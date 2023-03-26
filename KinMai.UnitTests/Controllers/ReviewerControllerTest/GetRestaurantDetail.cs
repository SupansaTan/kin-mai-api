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
using KinMai.S3.UnitOfWork.Interface;
using KinMai.UnitTests.Shared;
using Moq;
using Newtonsoft.Json;
using System.Linq.Expressions;

namespace KinMai.UnitTests.Controllers.ReviewerControllerTest
{
    public class GetRestaurantDetail
    {
        private readonly InitConfiguration initConfiguration;
        private readonly Mock<IDapperUnitOfWork> mockDapperUnitOfWork;
        private readonly Mock<IS3UnitOfWork> mockS3UnitOfWork;
        private readonly Mock<IMailUnitOfWork> mockMailUnitOfWork;
        private readonly Mock<IEntityUnitOfWork> mockEntityUnitOfWork;
        private readonly Mock<IAuthenticationUnitOfWork> mockAuthenticationUnitOfWork;
        private readonly ILogicUnitOfWork logicUnitOfWork;
        private readonly ReviewerController reviewerController;

        public GetRestaurantDetail()
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
            reviewerController = new ReviewerController(logicUnitOfWork);
        }

        [Fact]
        public async Task GetRestaurantDetail_ReturnStatus200_WhenRequestWithValidModel()
        {
            // arrange
            var mockRestaurantInfo = new List<GetRestaurantDetailModel>() {
                new GetRestaurantDetailModel()
                {
                    RestaurantId = Guid.NewGuid(),
                    RestaurantName = "Test",
                    Rating = 5,
                    StartTime = "09:00",
                    EndTime = "23:59",
                    Distance = 500,
                    MinPriceRate = 200,
                    MaxPriceRate = 1000,
                    TotalReview = 0,
                    ImageCover = new List<string>() { "testImageCover" },
                    Address = "11111 Pracharat Rd.",
                    DeliveryTypeList = new List<int>() { 1,2 },
                    CategoryList = new List<int>() { 1,2 },
                    FoodRecommendList = new List<string>() { "Coffee" },
                    Description = "Good Taste, Good mood",
                    PaymentMethodList = new List<int>() { 1,2 },
                    SocialContactList = new List<string>()
                    {
                        JsonConvert.SerializeObject(new SocialContactItemModel()
                        {
                            SocialType = SocialContactType.Tel,
                            ContactValue = "0888888888"
                        })
                    },
                    IsReview = false,
                    Latitude = 0,
                    Longitude = 0,
                }
            };

            var mockRestaurant = new Restaurant()
            {
                Id = Guid.NewGuid(),
                OwnerId = Guid.NewGuid(),
                Name = "Test",
                RestaurantType = (int)RestaurantType.All,
                MinPriceRate = 200,
                MaxPriceRate = 1000,
                Address = "11111 Pracharat Rd.",
                ImageLink = mockRestaurantInfo[0].ImageCover.ToArray(),
                DeliveryType = mockRestaurantInfo[0].DeliveryTypeList.ToArray(),
                PaymentMethod = mockRestaurantInfo[0].PaymentMethodList.ToArray(),
                Description = "Good Taste, Good mood",
                Latitude = 0,
                Longitude = 0,
                CreateAt = DateTime.Now
            };

            // setup query & dapper response
            mockEntityUnitOfWork.Setup(x => x.RestaurantRepository.GetSingleAsync(It.IsAny<Expression<Func<Restaurant, bool>>>()))
                                .ReturnsAsync(() => mockRestaurant);
            mockDapperUnitOfWork.Setup(x => x.KinMaiRepository.QueryAsync<GetRestaurantDetailModel>(It.IsAny<string>()))
                                .Returns(() => Task.FromResult<IEnumerable<GetRestaurantDetailModel>>(mockRestaurantInfo));

            var mockRequest = new GetRestaurantDetailRequestModel()
            {
                UserId = Guid.NewGuid(),
                RestaurantId = Guid.NewGuid(),
                Latitude = 0,
                Longitude = 0,
            };

            // act
            var actualOutput = await reviewerController.GetRestaurantDetail(mockRequest);
            var expectedOutput = new ResponseModel<GetRestaurantDetailModel>()
            {
                Data = mockRestaurantInfo[0],
                Status = 200,
                Message = "success"
            };

            // assert
            var actualOutputObj = JsonConvert.SerializeObject(actualOutput);
            var expetedOutputObj = JsonConvert.SerializeObject(expectedOutput);
            Assert.Equal(expetedOutputObj, actualOutputObj);
        }

        [Fact]
        public async Task GetRestaurantDetail_ReturnStatus200_WhenRequestByVisitor()
        {
            // arrange
            var mockRestaurantInfo = new List<GetRestaurantDetailModel>() {
                new GetRestaurantDetailModel()
                {
                    RestaurantId = Guid.NewGuid(),
                    RestaurantName = "Test",
                    Rating = 5,
                    StartTime = "09:00",
                    EndTime = "23:59",
                    Distance = 500,
                    MinPriceRate = 200,
                    MaxPriceRate = 1000,
                    TotalReview = 0,
                    ImageCover = new List<string>() { "testImageCover" },
                    Address = "11111 Pracharat Rd.",
                    DeliveryTypeList = new List<int>() { 1,2 },
                    CategoryList = new List<int>() { 1,2 },
                    FoodRecommendList = new List<string>() { "Coffee" },
                    Description = "Good Taste, Good mood",
                    PaymentMethodList = new List<int>() { 1,2 },
                    SocialContactList = new List<string>()
                    {
                        JsonConvert.SerializeObject(new SocialContactItemModel()
                        {
                            SocialType = SocialContactType.Tel,
                            ContactValue = "0888888888"
                        })
                    },
                    IsReview = false,
                    Latitude = 0,
                    Longitude = 0,
                }
            };

            var mockRestaurant = new Restaurant()
            {
                Id = Guid.NewGuid(),
                OwnerId = Guid.NewGuid(),
                Name = "Test",
                RestaurantType = (int)RestaurantType.All,
                MinPriceRate = 200,
                MaxPriceRate = 1000,
                Address = "11111 Pracharat Rd.",
                ImageLink = mockRestaurantInfo[0].ImageCover.ToArray(),
                DeliveryType = mockRestaurantInfo[0].DeliveryTypeList.ToArray(),
                PaymentMethod = mockRestaurantInfo[0].PaymentMethodList.ToArray(),
                Description = "Good Taste, Good mood",
                Latitude = 0,
                Longitude = 0,
                CreateAt = DateTime.Now
            };

            // setup query & dapper response
            mockEntityUnitOfWork.Setup(x => x.RestaurantRepository.GetSingleAsync(It.IsAny<Expression<Func<Restaurant, bool>>>()))
                                .ReturnsAsync(() => mockRestaurant);
            mockDapperUnitOfWork.Setup(x => x.KinMaiRepository.QueryAsync<GetRestaurantDetailModel>(It.IsAny<string>()))
                                .Returns(() => Task.FromResult<IEnumerable<GetRestaurantDetailModel>>(mockRestaurantInfo));

            var mockRequest = new GetRestaurantDetailRequestModel()
            {
                RestaurantId = Guid.NewGuid(),
                Latitude = 0,
                Longitude = 0,
            };

            // act
            var actualOutput = await reviewerController.GetRestaurantDetail(mockRequest);
            var expectedOutput = new ResponseModel<GetRestaurantDetailModel>()
            {
                Data = mockRestaurantInfo[0],
                Status = 200,
                Message = "success"
            };

            // assert
            var actualOutputObj = JsonConvert.SerializeObject(actualOutput);
            var expetedOutputObj = JsonConvert.SerializeObject(expectedOutput);
            Assert.Equal(expetedOutputObj, actualOutputObj);
        }

        [Fact]
        public async Task GetRestaurantDetail_ReturnStatus400_WhenRestaurantDoesNotExist()
        {
            // setup query & dapper response
            mockEntityUnitOfWork.Setup(x => x.RestaurantRepository.GetSingleAsync(It.IsAny<Expression<Func<Restaurant, bool>>>()))
                                .ReturnsAsync(() => null);

            // arrange
            var mockRequest = new GetRestaurantDetailRequestModel()
            {
                UserId = Guid.NewGuid(),
                RestaurantId = Guid.NewGuid(),
                Latitude = 0,
                Longitude = 0,
            };

            // act
            var actualOutput = await reviewerController.GetRestaurantDetail(mockRequest);
            var expectedOutput = new ResponseModel<GetRestaurantDetailModel>()
            {
                Data = null,
                Status = 400,
                Message = "Restaurant does not exist."
            };

            // assert
            var actualOutputObj = JsonConvert.SerializeObject(actualOutput);
            var expetedOutputObj = JsonConvert.SerializeObject(expectedOutput);
            Assert.Equal(expetedOutputObj, actualOutputObj);
        }

        [Fact]
        public async Task GetRestaurantDetail_ReturnStatus500_WhenRequestWithInValidModel()
        {
            // arrange
            var mockRequest = new GetRestaurantDetailRequestModel()
            {
                UserId = Guid.NewGuid(),
                Latitude = 0,
                Longitude = 0,
            };

            // act
            var actualOutput = await reviewerController.GetRestaurantDetail(mockRequest);
            var expectedOutput = new ResponseModel<GetRestaurantDetailModel>()
            {
                Data = null,
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
