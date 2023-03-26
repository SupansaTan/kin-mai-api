using KinMai.Api.Controllers;
using KinMai.Api.Models;
using KinMai.Authentication.UnitOfWork;
using KinMai.Common.Enum;
using KinMai.Dapper.Interface;
using KinMai.EntityFramework.Models;
using KinMai.EntityFramework.UnitOfWork.Interface;
using KinMai.Logic.Models;
using KinMai.Logic.Services;
using KinMai.Logic.UnitOfWork.Implement;
using KinMai.Logic.UnitOfWork.Interface;
using KinMai.Mail.UnitOfWork;
using KinMai.S3.Models;
using KinMai.S3.UnitOfWork.Interface;
using KinMai.UnitTests.Shared;
using Microsoft.AspNetCore.Http;
using Moq;
using Newtonsoft.Json;
using System.Linq.Expressions;

namespace KinMai.UnitTests.Controllers.RestaurantControllerTest
{
    public class UpdateRestuarantDatail
    {
        private readonly InitConfiguration initConfiguration;
        private readonly Mock<IDapperUnitOfWork> mockDapperUnitOfWork;
        private readonly Mock<IS3UnitOfWork> mockS3UnitOfWork;
        private readonly Mock<IMailUnitOfWork> mockMailUnitOfWork;
        private readonly Mock<IAuthenticationUnitOfWork> mockAuthenticationUnitOfWork;
        private readonly Mock<IEntityUnitOfWork> mockEntityUnitOfWork;
        private readonly ILogicUnitOfWork logicUnitOfWork;
        private readonly RestaurantController restaurantController;

        public UpdateRestuarantDatail()
        {
            initConfiguration = new InitConfiguration();
            mockDapperUnitOfWork = new Mock<IDapperUnitOfWork>();
            mockS3UnitOfWork = new Mock<IS3UnitOfWork>();
            mockMailUnitOfWork = new Mock<IMailUnitOfWork>();
            mockEntityUnitOfWork = new Mock<IEntityUnitOfWork>();
            mockAuthenticationUnitOfWork = new Mock<IAuthenticationUnitOfWork>();
            logicUnitOfWork = new LogicUnitOfWork(
                mockEntityUnitOfWork.Object,
                mockDapperUnitOfWork.Object,
                mockAuthenticationUnitOfWork.Object,
                mockS3UnitOfWork.Object,
                mockMailUnitOfWork.Object
            );
            restaurantController = new RestaurantController(logicUnitOfWork);
        }

        [Fact]
        public async Task UpdateRestuarantDatail_ReturnStatus200_WhenRequestValidModel()
        {
            var mockRestaurant = new Restaurant()
            {
                Id = Guid.NewGuid(),
                OwnerId = Guid.NewGuid(),
                Name = "Somtam",
                Description = "Good taste, Good mood",
                Address = "1111 Pracharat Rd.",
                CreateAt = DateTime.UtcNow,
                RestaurantType = (int)RestaurantType.All,
                Latitude = 100,
                Longitude = 36,
                MinPriceRate = 100,
                MaxPriceRate = 300,
                ImageLink = new List<string>() { "imageLink" }.ToArray(),
                DeliveryType = new List<int>() { 1, 2 }.ToArray(),
                PaymentMethod = new List<int>() { 1, 2 }.ToArray()
            };

            var mockCategory = new Category()
            {
                Id = Guid.NewGuid(),
                Name = "Cafe",
                Type = 12,
            };

            var mockRestaurantInfoResponse = new List<ResArrayDataModel>()
            {
                new ResArrayDataModel()
                {
                    ImageLink = mockRestaurant.ImageLink,
                    DeliveryType = mockRestaurant.DeliveryType,
                    PaymentMethod = mockRestaurant.PaymentMethod,
                }
            };

            // mock image file
            byte[] filebytes = File.ReadAllBytes($"{AppDomain.CurrentDomain.BaseDirectory}/Images/restaurant.jpg");
            IFormFile imageFileMock = new FormFile(new MemoryStream(filebytes), 0, filebytes.Length, "Data", "restaurant.png");

            var mockRequest = new RestaurantUpdateModel()
            {
                RestaurantId = mockRestaurant.Id,
                RestaurantStatus = "Good Taste, Let's try!",
                NewImageFile = new List<IFormFile> { imageFileMock },
                RemoveImageLink = new List<string> { "imageLink" },
                ResUpdateInfo = new RestaurantInfoModel()
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
                    Contact = new List<RestaurantContactModel>
                    {
                        new RestaurantContactModel()
                        {
                            Social = SocialContactType.Tel,
                            ContactValue = "0888888888"
                        }
                    },
                    BusinessHours = new List<BusinessHourModel>
                    {
                        new BusinessHourModel()
                        {
                            Day = 1,
                            StartTime = DateTime.Now,
                            EndTime = DateTime.Now.AddHours(7),
                        }
                    }
                }
            };

            // setup db response
            mockEntityUnitOfWork.Setup(x => x.RestaurantRepository.GetSingleAsync(It.IsAny<Expression<Func<Restaurant, bool>>>()))
                                .ReturnsAsync(() => mockRestaurant);
            mockEntityUnitOfWork.Setup(x => x.CategoryRepository.GetSingleAsync(It.IsAny<Expression<Func<Category, bool>>>()))
                                .ReturnsAsync(() => mockCategory);
            mockEntityUnitOfWork.Setup(x => x.RestaurantRepository.Update(It.IsAny<Restaurant>()));
            mockEntityUnitOfWork.Setup(x => x.BusinessHourRepository.AddRange(It.IsAny<List<BusinessHour>>()));
            mockEntityUnitOfWork.Setup(x => x.SocialContactRepository.AddRange(It.IsAny<List<SocialContact>>()));
            mockEntityUnitOfWork.Setup(x => x.RelatedRepository.AddRange(It.IsAny<List<Related>>()));
            mockEntityUnitOfWork.Setup(x => x.SaveAsync());

            // setup dapper response
            mockDapperUnitOfWork.Setup(x => x.KinMaiRepository.QueryAsync<ResArrayDataModel>(It.IsAny<string>()))
                                .Returns(() => Task.FromResult<IEnumerable<ResArrayDataModel>>(mockRestaurantInfoResponse));

            // set up s3 response
            mockS3UnitOfWork.Setup(x => x.S3FileService.UploadImage(It.IsAny<UploadImageModel>()))
                            .Returns(Task.FromResult("image link"));


            // act
            var actualOutput = await restaurantController.UpdateRestaurantDetail(mockRequest);
            var expectedOutput = new ResponseModel<bool>()
            {
                Data = true,
                Status = 200,
                Message = "success"
            };

            // assert
            var actualOutputObj = JsonConvert.SerializeObject(actualOutput);
            var expetedOutputObj = JsonConvert.SerializeObject(expectedOutput);
            Assert.Equal(expetedOutputObj, actualOutputObj);
        }

        [Fact]
        public async Task UpdateRestuarantDatail_ReturnStatus400_WhenRestaurantDoesNotExist()
        {
            // mock image file
            byte[] filebytes = File.ReadAllBytes($"{AppDomain.CurrentDomain.BaseDirectory}/Images/restaurant.jpg");
            IFormFile imageFileMock = new FormFile(new MemoryStream(filebytes), 0, filebytes.Length, "Data", "restaurant.png");

            var mockRequest = new RestaurantUpdateModel()
            {
                RestaurantId = Guid.NewGuid(),
                RestaurantStatus = "Good Taste, Let's try!",
                NewImageFile = new List<IFormFile> { imageFileMock },
                RemoveImageLink = new List<string> { "imageLink" },
                ResUpdateInfo = new RestaurantInfoModel()
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
                    Contact = new List<RestaurantContactModel>
                    {
                        new RestaurantContactModel()
                        {
                            Social = SocialContactType.Tel,
                            ContactValue = "0888888888"
                        }
                    },
                    BusinessHours = new List<BusinessHourModel>
                    {
                        new BusinessHourModel()
                        {
                            Day = 1,
                            StartTime = DateTime.Now,
                            EndTime = DateTime.Now.AddHours(7),
                        }
                    }
                }
            };

            // setup db response
            mockEntityUnitOfWork.Setup(x => x.RestaurantRepository.GetSingleAsync(It.IsAny<Expression<Func<Restaurant, bool>>>()))
                                .ReturnsAsync(() => null);

            // act
            var actualOutput = await restaurantController.UpdateRestaurantDetail(mockRequest);
            var expectedOutput = new ResponseModel<bool>()
            {
                Data = false,
                Status = 400,
                Message = "This restaurant does not exists."
            };

            // assert
            var actualOutputObj = JsonConvert.SerializeObject(actualOutput);
            var expetedOutputObj = JsonConvert.SerializeObject(expectedOutput);
            Assert.Equal(expetedOutputObj, actualOutputObj);
        }

        [Fact]
        public async Task UpdateRestuarantDatail_ReturnStatus500_WhenCategoryDoesNotExist()
        {
            var mockRestaurant = new Restaurant()
            {
                Id = Guid.NewGuid(),
                OwnerId = Guid.NewGuid(),
                Name = "Somtam",
                Description = "Good taste, Good mood",
                Address = "1111 Pracharat Rd.",
                CreateAt = DateTime.UtcNow,
                RestaurantType = (int)RestaurantType.All,
                Latitude = 100,
                Longitude = 36,
                MinPriceRate = 100,
                MaxPriceRate = 300,
                ImageLink = new List<string>() { "imageLink" }.ToArray(),
                DeliveryType = new List<int>() { 1, 2 }.ToArray(),
                PaymentMethod = new List<int>() { 1, 2 }.ToArray()
            };

            var mockRestaurantInfoResponse = new List<ResArrayDataModel>()
            {
                new ResArrayDataModel()
                {
                    ImageLink = mockRestaurant.ImageLink,
                    DeliveryType = mockRestaurant.DeliveryType,
                    PaymentMethod = mockRestaurant.PaymentMethod,
                }
            };

            // mock image file
            byte[] filebytes = File.ReadAllBytes($"{AppDomain.CurrentDomain.BaseDirectory}/Images/restaurant.jpg");
            IFormFile imageFileMock = new FormFile(new MemoryStream(filebytes), 0, filebytes.Length, "Data", "restaurant.png");

            var mockRequest = new RestaurantUpdateModel()
            {
                RestaurantId = mockRestaurant.Id,
                RestaurantStatus = "Good Taste, Let's try!",
                NewImageFile = new List<IFormFile> { imageFileMock },
                RemoveImageLink = new List<string> { "imageLink" },
                ResUpdateInfo = new RestaurantInfoModel()
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
                    Contact = new List<RestaurantContactModel>
                    {
                        new RestaurantContactModel()
                        {
                            Social = SocialContactType.Tel,
                            ContactValue = "0888888888"
                        }
                    },
                    BusinessHours = new List<BusinessHourModel>
                    {
                        new BusinessHourModel()
                        {
                            Day = 1,
                            StartTime = DateTime.Now,
                            EndTime = DateTime.Now.AddHours(7),
                        }
                    }
                }
            };

            // setup db response
            mockEntityUnitOfWork.Setup(x => x.RestaurantRepository.GetSingleAsync(It.IsAny<Expression<Func<Restaurant, bool>>>()))
                                .ReturnsAsync(() => mockRestaurant);
            mockEntityUnitOfWork.Setup(x => x.CategoryRepository.GetSingleAsync(It.IsAny<Expression<Func<Category, bool>>>()))
                                .ReturnsAsync(() => null);

            // setup dapper response
            mockDapperUnitOfWork.Setup(x => x.KinMaiRepository.QueryAsync<ResArrayDataModel>(It.IsAny<string>()))
                                .Returns(() => Task.FromResult<IEnumerable<ResArrayDataModel>>(mockRestaurantInfoResponse));

            // act
            var actualOutput = await restaurantController.UpdateRestaurantDetail(mockRequest);
            var expectedOutput = new ResponseModel<bool>()
            {
                Data = false,
                Status = 500,
                Message = "Object reference not set to an instance of an object."
            };

            // assert
            var actualOutputObj = JsonConvert.SerializeObject(actualOutput);
            var expetedOutputObj = JsonConvert.SerializeObject(expectedOutput);
            Assert.Equal(expetedOutputObj, actualOutputObj);
        }
    }
}
