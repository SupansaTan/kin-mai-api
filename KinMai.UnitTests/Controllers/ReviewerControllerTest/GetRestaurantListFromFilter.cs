using KinMai.Api.Controllers;
using KinMai.Api.Models;
using KinMai.Api.Models.Reviewer;
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

namespace KinMai.UnitTests.Controllers.ReviewerControllerTest
{
    public class GetRestaurantListFromFilter
	{
        private readonly InitConfiguration initConfiguration;
        private readonly Mock<IDapperUnitOfWork> mockDapperUnitOfWork;
        private readonly Mock<IS3UnitOfWork> mockS3UnitOfWork;
        private readonly Mock<IMailUnitOfWork> mockMailUnitOfWork;
        private readonly Mock<IEntityUnitOfWork> mockEntityUnitOfWork;
        private readonly Mock<IAuthenticationUnitOfWork> mockAuthenticationUnitOfWork;
        private readonly ILogicUnitOfWork logicUnitOfWork;
        private readonly ReviewerController reviewerController;

        public GetRestaurantListFromFilter()
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
        public async Task GetRestaurantListFromFilter_RetuenStatus200_WhenRequestWithValidModel()
        {
            var mockRestaurantCard = new List<RestaurantCardInfoModel>()
            {
                new RestaurantCardInfoModel()
                {
                    RestaurantId = Guid.NewGuid(),
                    RestaurantName = "Test",
                    Rating = 5,
                    StartTime = "09:00",
                    EndTime = "23:59",
                    Distance = 500,
                    MinPriceRate = 200,
                    MaxPriceRate = 1000,
                    ImageCover = "testImageCover",
                    Description = "TestDescription",
                    TotalReview = 0,
                    IsFavorite = false,
                    IsReview = false
                }
            };

            var mockRestaurant = new List<Restaurant>()
            {
                new Restaurant()
                {
                    Id = Guid.NewGuid(),
                    OwnerId = Guid.NewGuid(),
                    Name = "Test",
                    RestaurantType = (int)RestaurantType.All,
                    MinPriceRate = 200,
                    MaxPriceRate = 1000,
                    Latitude = 0,
                    Longitude = 0,
                    CreateAt = DateTime.Now
                }
            };

            IQueryable<Restaurant> queryableRestaurant = mockRestaurant.AsQueryable();

            // arrange mock req
            var mockRequest = new GetRestaurantListFromFilterRequestModel()
            {
                userId = Guid.NewGuid(),
                latitude = 13.736717,
                longitude = 100.523186,
                skip = 0,
                take = 20,
                Keywords = "Test",
                IsOpen = true,
                CategoryType = new List<int>() { 1, 2 }.ToArray(),
                DeliveryType = new List<int>() { 1, 2 }.ToArray(),
                PaymentMethod = new List<int>() { 1, 2 }.ToArray()

            };

            // setup db & dapper response



            // act
            var actualOutput = await reviewerController.GetRestaurantListFromFilter(mockRequest);
            var expectOutput = new ResponseModel<RestaurantCardListModel>
            {
                Data = new RestaurantCardListModel()
                {
                    RestaurantInfo = mockRestaurantCard,
                    TotalRestaurant = 1,
                    RestaurantCumulativeCount = 1
                },
                Message = "success",
                Status = 200
            };

            // assert
            var actualOutputObj = JsonConvert.SerializeObject(actualOutput);
            var expetedOutputObj = JsonConvert.SerializeObject(expectOutput);
            Assert.Equal(expetedOutputObj, actualOutputObj);

        }

        [Fact]
        public async Task GetRestaurantListFromFilter_RetuenStatus500_WhenRequestWithInValidModel()
        {
            // arrange mock req
            var mockRequest = new GetRestaurantListFromFilterRequestModel()
            {
                userId = Guid.NewGuid(),
                latitude = 0,
                longitude = 0,
            };

            // act
            var actualOutput = await reviewerController.GetRestaurantListFromFilter(mockRequest);
            var expectOutput = new ResponseModel<RestaurantCardListModel>
            {
                Data = null,
                Message = "Object reference not set to an instance of an object.",
                Status = 500
            };

            // assert
            var actualOutputObj = JsonConvert.SerializeObject(actualOutput);
            var expetedOutputObj = JsonConvert.SerializeObject(expectOutput);
            Assert.Equal(expetedOutputObj, actualOutputObj);
        }

    }
}

