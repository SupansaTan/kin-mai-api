using Amazon.CognitoIdentityProvider.Model;
using KinMai.Api.Controllers;
using KinMai.Api.Models;
using KinMai.Authentication.UnitOfWork;
using KinMai.Common.Enum;
using KinMai.Dapper.Interface;
using KinMai.EntityFramework.Models;
using KinMai.EntityFramework.UnitOfWork.Interface;
using KinMai.Logic.Models;
using KinMai.Logic.UnitOfWork.Implement;
using KinMai.Logic.UnitOfWork.Interface;
using KinMai.Mail.UnitOfWork;
using KinMai.S3.Models;
using KinMai.S3.UnitOfWork.Interface;
using KinMai.UnitTests.Shared;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Linq.Expressions;
using System.Net;

namespace KinMai.UnitTests.Controllers.AuthenticationControllerTest
{
    public class RestaurantRegister
    {
        private readonly InitConfiguration initConfiguration;
        private readonly Mock<IDapperUnitOfWork> mockDapperUnitOfWork;
        private readonly Mock<IS3UnitOfWork> mockS3UnitOfWork;
        private readonly Mock<IMailUnitOfWork> mockMailUnitOfWork;
        private readonly Mock<IAuthenticationUnitOfWork> mockAuthenticationUnitOfWork;
        private readonly Mock<IEntityUnitOfWork> mockEntityUnitOfWork;
        private readonly ILogicUnitOfWork logicUnitOfWork;
        private readonly AuthenticationController authenticationController;

        public RestaurantRegister()
        {
            initConfiguration = new InitConfiguration();
            mockDapperUnitOfWork = new Mock<IDapperUnitOfWork>();
            mockS3UnitOfWork = new Mock<IS3UnitOfWork>();
            mockMailUnitOfWork = new Mock<IMailUnitOfWork>();
            mockAuthenticationUnitOfWork = new Mock<IAuthenticationUnitOfWork>();
            mockEntityUnitOfWork = new Mock<IEntityUnitOfWork>();
            logicUnitOfWork = new LogicUnitOfWork(
                mockEntityUnitOfWork.Object,
                mockDapperUnitOfWork.Object,
                mockAuthenticationUnitOfWork.Object,
                mockS3UnitOfWork.Object,
                mockMailUnitOfWork.Object
            );
            authenticationController = new AuthenticationController(logicUnitOfWork);
        }

        [Fact]
        public async Task RestaurantRegister_ReturnStatus200_WhenRegisterWithValidModel()
        {
            // arrange
            var mockUserInfo = new ReviewerRegisterModel()
            {
                Email = "nampunch1@gmail.com",
                FirstName = "Supansa",
                LastName = "Tantulset",
                Username = "littlepunchhz",
                Password = "12345678",
                ConfirmPassword = "12345678"
            };

            var mockRestaurantInfo = new RestaurantInfoModel()
            {
                RestaurantName = "Somtam",
                minPriceRate = 100,
                maxPriceRate = 500,
                Address = new RestaurantAddressModel()
                {
                    Address = "11111 Pracharat Rd.",
                    Latitude = 10,
                    Longitude = 10,
                },
                RestaurantType = RestaurantType.All,
                DeliveryType = new List<int> { 1, 2 },
                Categories = new List<RestaurantCategories> { RestaurantCategories.Cafe, RestaurantCategories.Bakery },
                PaymentMethods = new List<int> { 1, 2 },
                Contact = new List<RestaurantContactModel> {
                    new RestaurantContactModel()
                    {
                        Social = SocialContactType.Tel,
                        ContactValue = "0888888888"
                    }
                },
                BusinessHours = new List<BusinessHourModel> {
                    new BusinessHourModel()
                    {
                        Day = 1,
                        StartTime = DateTime.Now,
                        EndTime = DateTime.Now.AddHours(7),
                    }
                }
            };

            // mock image file
            byte[] filebytes = File.ReadAllBytes($"{AppDomain.CurrentDomain.BaseDirectory}/Images/restaurant.jpg");
            IFormFile imageFileMock = new FormFile(new MemoryStream(filebytes), 0, filebytes.Length, "Data", "restaurant.png");

            var mockRestaurantAdditionInfo = new RestaurantPhotoModel()
            {
                ImageFiles = new List<IFormFile>() { imageFileMock },
                RestaurantStatus = "Good taste ever!"
            };

            var request = new RestaurantRegisterModel()
            {
                PersonalInfo = mockUserInfo,
                RestaurantInfo = mockRestaurantInfo,
                RestaurantAdditionInfo = mockRestaurantAdditionInfo
            };

            var mockSignUpResponse = new SignUpResponse()
            {
                HttpStatusCode = HttpStatusCode.OK
            };

            // setup database response
            mockEntityUnitOfWork.Setup(x => x.UserRepository.GetSingleAsync(It.IsAny<Expression<Func<User, bool>>>()))
                                .ReturnsAsync(() => null);
            mockEntityUnitOfWork.Setup(x => x.CategoryRepository.GetSingleAsync(It.IsAny<Expression<Func<Category, bool>>>()))
                                .ReturnsAsync(() => new Category()
                                {
                                    Id = Guid.NewGuid(),
                                    Name = "Cafe",
                                    Type = 12
                                });
            mockEntityUnitOfWork.Setup(x => x.UserRepository.Add(It.IsAny<User>()));
            mockEntityUnitOfWork.Setup(x => x.RestaurantRepository.Add(It.IsAny<Restaurant>()));
            mockEntityUnitOfWork.Setup(x => x.BusinessHourRepository.AddRange(It.IsAny<List<BusinessHour>>()));
            mockEntityUnitOfWork.Setup(x => x.SocialContactRepository.AddRange(It.IsAny<List<SocialContact>>()));
            mockEntityUnitOfWork.Setup(x => x.RelatedRepository.AddRange(It.IsAny<List<Related>>()));

            // setup aws cognito response
            mockAuthenticationUnitOfWork.Setup(x => x.AWSCognitoService.SignUp(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
                                        .Returns(Task.FromResult(mockSignUpResponse));
            mockAuthenticationUnitOfWork.Setup(x => x.AWSCognitoService.ConfirmSignUp(It.IsAny<Guid>()))
                                        .Returns(Task.FromResult(true));

            // set up s3 response
            mockS3UnitOfWork.Setup(x => x.S3FileService.UploadImage(It.IsAny<UploadImageModel>()))
                            .Returns(Task.FromResult("image link"));

            // act
            var actualOutput = await authenticationController.RestaurantRegister(request);
            var expectOutput = new ResponseModel<bool>()
            {
                Data = true,
                Message = "success",
                Status = 200
            };

            // assert
            Assert.Equal(actualOutput.Data, expectOutput.Data);
            Assert.Equal(actualOutput.Message, expectOutput.Message);
            Assert.Equal(actualOutput.Status, expectOutput.Status);
        }

        [Fact]
        public async Task RestaurantRegister_ReturnStatus200_WhenRegisterWithGoogleAccount()
        {
            // arrange
            var mockUserInfo = new ReviewerRegisterModel()
            {
                Email = "nampunch1@gmail.com",
                FirstName = "Supansa",
                LastName = "Tantulset",
                Username = "littlepunchhz",
            };

            var mockRestaurantInfo = new RestaurantInfoModel()
            {
                RestaurantName = "Somtam",
                minPriceRate = 100,
                maxPriceRate = 500,
                Address = new RestaurantAddressModel()
                {
                    Address = "11111 Pracharat Rd.",
                    Latitude = 10,
                    Longitude = 10,
                },
                RestaurantType = RestaurantType.All,
                DeliveryType = new List<int> { 1, 2 },
                Categories = new List<RestaurantCategories> { RestaurantCategories.Cafe, RestaurantCategories.Bakery },
                PaymentMethods = new List<int> { 1, 2 },
                Contact = new List<RestaurantContactModel> {
                    new RestaurantContactModel()
                    {
                        Social = SocialContactType.Tel,
                        ContactValue = "0888888888"
                    }
                },
                BusinessHours = new List<BusinessHourModel> {
                    new BusinessHourModel()
                    {
                        Day = 1,
                        StartTime = DateTime.Now,
                        EndTime = DateTime.Now.AddHours(7),
                    }
                }
            };

            // mock image file
            byte[] filebytes = File.ReadAllBytes($"{AppDomain.CurrentDomain.BaseDirectory}/Images/restaurant.jpg");
            IFormFile imageFileMock = new FormFile(new MemoryStream(filebytes), 0, filebytes.Length, "Data", "restaurant.png");

            var mockRestaurantAdditionInfo = new RestaurantPhotoModel()
            {
                ImageFiles = new List<IFormFile>() { imageFileMock },
                RestaurantStatus = "Good taste ever!"
            };

            var request = new RestaurantRegisterModel()
            {
                PersonalInfo = mockUserInfo,
                RestaurantInfo = mockRestaurantInfo,
                RestaurantAdditionInfo = mockRestaurantAdditionInfo
            };

            // setup database response
            mockEntityUnitOfWork.Setup(x => x.UserRepository.GetSingleAsync(It.IsAny<Expression<Func<User, bool>>>()))
                                .ReturnsAsync(() => null);
            mockEntityUnitOfWork.Setup(x => x.CategoryRepository.GetSingleAsync(It.IsAny<Expression<Func<Category, bool>>>()))
                                .ReturnsAsync(() => new Category()
                                {
                                    Id = Guid.NewGuid(),
                                    Name = "Cafe",
                                    Type = 12
                                });
            mockEntityUnitOfWork.Setup(x => x.UserRepository.Add(It.IsAny<User>()));
            mockEntityUnitOfWork.Setup(x => x.RestaurantRepository.Add(It.IsAny<Restaurant>()));
            mockEntityUnitOfWork.Setup(x => x.BusinessHourRepository.AddRange(It.IsAny<List<BusinessHour>>()));
            mockEntityUnitOfWork.Setup(x => x.SocialContactRepository.AddRange(It.IsAny<List<SocialContact>>()));
            mockEntityUnitOfWork.Setup(x => x.RelatedRepository.AddRange(It.IsAny<List<Related>>()));

            // set up s3 response
            mockS3UnitOfWork.Setup(x => x.S3FileService.UploadImage(It.IsAny<UploadImageModel>()))
                            .Returns(Task.FromResult("image link"));

            // act
            var actualOutput = await authenticationController.RestaurantRegister(request);
            var expectOutput = new ResponseModel<bool>()
            {
                Data = true,
                Message = "success",
                Status = 200
            };

            // assert
            Assert.Equal(actualOutput.Data, expectOutput.Data);
            Assert.Equal(actualOutput.Message, expectOutput.Message);
            Assert.Equal(actualOutput.Status, expectOutput.Status);
        }

        [Fact]
        public async Task RestaurantRegister_ReturnStatus400_WhenPasswordDoNotMatch()
        {
            // arrange
            var mockUserInfo = new ReviewerRegisterModel()
            {
                Email = "nampunch1@gmail.com",
                FirstName = "Supansa",
                LastName = "Tantulset",
                Username = "littlepunchhz",
                Password = "12345678",
                ConfirmPassword = "1234566789"
            };

            var mockRestaurantInfo = new RestaurantInfoModel()
            {
                RestaurantName = "Somtam",
                minPriceRate = 100,
                maxPriceRate = 500,
                Address = new RestaurantAddressModel()
                {
                    Address = "11111 Pracharat Rd.",
                    Latitude = 10,
                    Longitude = 10,
                },
                RestaurantType = RestaurantType.All,
                DeliveryType = new List<int> { 1, 2 },
                Categories = new List<RestaurantCategories> { RestaurantCategories.Cafe, RestaurantCategories.Bakery },
                PaymentMethods = new List<int> { 1, 2 },
                Contact = new List<RestaurantContactModel> {
                    new RestaurantContactModel()
                    {
                        Social = SocialContactType.Tel,
                        ContactValue = "0888888888"
                    }
                },
                BusinessHours = new List<BusinessHourModel> {
                    new BusinessHourModel()
                    {
                        Day = 1,
                        StartTime = DateTime.Now,
                        EndTime = DateTime.Now.AddHours(7),
                    }
                }
            };

            // mock image file
            byte[] filebytes = File.ReadAllBytes($"{AppDomain.CurrentDomain.BaseDirectory}/Images/restaurant.jpg");
            IFormFile imageFileMock = new FormFile(new MemoryStream(filebytes), 0, filebytes.Length, "Data", "restaurant.png");

            var mockRestaurantAdditionInfo = new RestaurantPhotoModel()
            {
                ImageFiles = new List<IFormFile>() { imageFileMock },
                RestaurantStatus = "Good taste ever!"
            };

            var request = new RestaurantRegisterModel()
            {
                PersonalInfo = mockUserInfo,
                RestaurantInfo = mockRestaurantInfo,
                RestaurantAdditionInfo = mockRestaurantAdditionInfo
            };

            // setup database response
            mockEntityUnitOfWork.Setup(x => x.UserRepository.GetSingleAsync(It.IsAny<Expression<Func<User, bool>>>()))
                                .ReturnsAsync(() => null);

            // act
            var actualOutput = await authenticationController.RestaurantRegister(request);
            var expectOutput = new ResponseModel<bool>()
            {
                Data = false,
                Message = "Password and Confirm password are not matching",
                Status = 400
            };

            // assert
            Assert.Equal(actualOutput.Data, expectOutput.Data);
            Assert.Equal(actualOutput.Message, expectOutput.Message);
            Assert.Equal(actualOutput.Status, expectOutput.Status);
        }

        [Fact]
        public async Task RestaurantRegister_ReturnStatus400_WhenRegisterWithExistEmail()
        {
            // arrange
            var mockUserInfo = new ReviewerRegisterModel()
            {
                Email = "nampunch1@gmail.com",
                FirstName = "Supansa",
                LastName = "Tantulset",
                Username = "littlepunchhz",
            };

            var mockRestaurantInfo = new RestaurantInfoModel()
            {
                RestaurantName = "Somtam",
                minPriceRate = 100,
                maxPriceRate = 500,
                Address = new RestaurantAddressModel()
                {
                    Address = "11111 Pracharat Rd.",
                    Latitude = 10,
                    Longitude = 10,
                },
                RestaurantType = RestaurantType.All,
                DeliveryType = new List<int> { 1, 2 },
                Categories = new List<RestaurantCategories> { RestaurantCategories.Cafe, RestaurantCategories.Bakery },
                PaymentMethods = new List<int> { 1, 2 },
                Contact = new List<RestaurantContactModel> {
                    new RestaurantContactModel()
                    {
                        Social = SocialContactType.Tel,
                        ContactValue = "0888888888"
                    }
                },
                BusinessHours = new List<BusinessHourModel> {
                    new BusinessHourModel()
                    {
                        Day = 1,
                        StartTime = DateTime.Now,
                        EndTime = DateTime.Now.AddHours(7),
                    }
                }
            };

            // mock image file
            byte[] filebytes = File.ReadAllBytes($"{AppDomain.CurrentDomain.BaseDirectory}/Images/restaurant.jpg");
            IFormFile imageFileMock = new FormFile(new MemoryStream(filebytes), 0, filebytes.Length, "Data", "restaurant.png");

            var mockRestaurantAdditionInfo = new RestaurantPhotoModel()
            {
                ImageFiles = new List<IFormFile>() { imageFileMock },
                RestaurantStatus = "Good taste ever!"
            };

            var request = new RestaurantRegisterModel()
            {
                PersonalInfo = mockUserInfo,
                RestaurantInfo = mockRestaurantInfo,
                RestaurantAdditionInfo = mockRestaurantAdditionInfo
            };

            // setup database response
            mockEntityUnitOfWork.Setup(x => x.UserRepository.GetSingleAsync(It.IsAny<Expression<Func<User, bool>>>()))
                                .ReturnsAsync(() => new User()
                                {
                                    Id = Guid.NewGuid(),
                                    Email = "nampunch1@gmail.com",
                                    FirstName = "Supansa",
                                    LastName = "Tantulset",
                                    Username = "littlepunchhz",
                                    CreateAt = DateTime.UtcNow,
                                    UserType = 1,
                                    IsLoginWithGoogle = false
                                });

            // act
            var actualOutput = await authenticationController.RestaurantRegister(request);
            var expectOutput = new ResponseModel<bool>()
            {
                Data = false,
                Message = "Email already exists.",
                Status = 400
            };

            // assert
            Assert.Equal(actualOutput.Data, expectOutput.Data);
            Assert.Equal(actualOutput.Message, expectOutput.Message);
            Assert.Equal(actualOutput.Status, expectOutput.Status);
        }
    }
}
