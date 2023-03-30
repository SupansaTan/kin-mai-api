using Amazon.CognitoIdentityProvider.Model;
using KinMai.Authentication.UnitOfWork;
using KinMai.Common.Enum;
using KinMai.Dapper.Interface;
using KinMai.EntityFramework.Models;
using KinMai.EntityFramework.UnitOfWork.Interface;
using KinMai.Logic.Interface;
using KinMai.Logic.Models;
using KinMai.Logic.Services;
using KinMai.Mail.UnitOfWork;
using KinMai.S3.Models;
using KinMai.S3.UnitOfWork.Interface;
using KinMai.UnitTests.Shared;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Linq.Expressions;
using System.Net;

namespace KinMai.UnitTests.Services.AuthenticationServiceTest
{
    public class RestaurantRegister
    {
        private readonly InitConfiguration initConfiguration;
        private Mock<IDapperUnitOfWork> mockDapperUnitOfWork;
        private Mock<IS3UnitOfWork> mockS3UnitOfWork;
        private Mock<IMailUnitOfWork> mockMailUnitOfWork;
        private Mock<IAuthenticationUnitOfWork> mockAuthenticationUnitOfWork;
        private Mock<IEntityUnitOfWork> mockEntityUnitOfWork;
        private IAuthenticationService authenticationService;

        public RestaurantRegister()
        {
            initConfiguration = new InitConfiguration();
            mockDapperUnitOfWork = new Mock<IDapperUnitOfWork>();
            mockS3UnitOfWork = new Mock<IS3UnitOfWork>();
            mockMailUnitOfWork = new Mock<IMailUnitOfWork>();
            mockAuthenticationUnitOfWork = new Mock<IAuthenticationUnitOfWork>();
            mockEntityUnitOfWork = new Mock<IEntityUnitOfWork>();
            authenticationService = new AuthenticationService(
                mockEntityUnitOfWork.Object,
                mockAuthenticationUnitOfWork.Object,
                mockDapperUnitOfWork.Object,
                mockS3UnitOfWork.Object,
                mockMailUnitOfWork.Object
            );
        }

        [Fact]
        public async Task RestaurantRegister_ReturnTrue_WhenRegisterWithValidModel()
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
            var actualOutput = await authenticationService.RestaurantRegister(request);

            // assert
            mockEntityUnitOfWork.VerifyAll();
            mockAuthenticationUnitOfWork.VerifyAll();
            mockS3UnitOfWork.VerifyAll();
            Assert.True(actualOutput);
        }

        [Fact]
        public async Task RestaurantRegister_ReturnTrue_WhenRegisterWithGoogleAccount()
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
            var actualOutput = await authenticationService.RestaurantRegister(request);

            // assert
            mockEntityUnitOfWork.VerifyAll();
            mockS3UnitOfWork.VerifyAll();
            Assert.True(actualOutput);
        }

        [Fact]
        public async Task RestaurantRegister_ThrowArgumentException_WhenPasswordDoNotMatch()
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
            Func<Task> act = () => authenticationService.RestaurantRegister(request);

            // assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(act);
            mockEntityUnitOfWork.VerifyAll();
            Assert.Equal("Password and Confirm password are not matching", exception.Message);
        }

        [Fact]
        public async Task RestaurantRegister_ThrowArgumentException_WhenRegisterWithExistEmail()
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
            Func<Task> act = () => authenticationService.RestaurantRegister(request);

            // assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(act);
            mockEntityUnitOfWork.VerifyAll();
            Assert.Equal("Email already exists.", exception.Message);
        }
    }
}
