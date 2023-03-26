using System;
using KinMai.Api.Controllers;
using KinMai.Api.Models;
using KinMai.Api.Models.Reviewer;
using KinMai.Dapper.Interface;
using KinMai.EntityFramework.UnitOfWork.Interface;
using KinMai.Logic.Interface;
using KinMai.Logic.Models;
using KinMai.Logic.Services;
using KinMai.S3.UnitOfWork.Interface;
using KinMai.UnitTests.Shared;
using Moq;
using Newtonsoft.Json;

namespace KinMai.UnitTests.Services.ReviewerServiceTest
{
	public class GetRestaurantListFromFilter
	{
        private readonly InitConfiguration initConfiguration;
        private readonly Mock<IDapperUnitOfWork> mockDapperUnitOfWork;
        private readonly Mock<IS3UnitOfWork> mockS3UnitOfWork;
        private readonly Mock<IEntityUnitOfWork> mockEntityUnitOfWork;
        private readonly IReviewerService reviewerService;

        public GetRestaurantListFromFilter()
        {
            initConfiguration = new InitConfiguration();
            mockDapperUnitOfWork = new Mock<IDapperUnitOfWork>();
            mockS3UnitOfWork = new Mock<IS3UnitOfWork>();
            mockEntityUnitOfWork = new Mock<IEntityUnitOfWork>();
            reviewerService = new ReviewerService(
                mockEntityUnitOfWork.Object,
                mockDapperUnitOfWork.Object,
                mockS3UnitOfWork.Object
            );
        }

        [Fact]
        public async Task GetRestaurantListFromFilter_RetuenRestaurantCardListModel_WhenRequestWithValidModel()
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
                CategoryType = new List<int>() { 1, 2 },
                DeliveryType = new List<int>() { 1, 2 },
                PaymentMethod = new List<int>() { 1, 2 }
            };

            // setup dapper response
            mockDapperUnitOfWork.Setup(x => x.KinMaiRepository.QueryAsync<RestaurantCardInfoModel>(It.IsAny<string>()))
                                .Returns(() => Task.FromResult<IEnumerable<RestaurantCardInfoModel>>(mockRestaurantCard));

            // act
            var actualOutput = await reviewerService.GetRestaurantListFromFilter(mockRequest);
            var expectOutput = new RestaurantCardListModel()
            {
                RestaurantInfo = mockRestaurantCard,
                TotalRestaurant = 1,
                RestaurantCumulativeCount = 1
            };

            // assert
            mockDapperUnitOfWork.VerifyAll();
            var actualOutputObj = JsonConvert.SerializeObject(actualOutput);
            var expetedOutputObj = JsonConvert.SerializeObject(expectOutput);
            Assert.Equal(expetedOutputObj, actualOutputObj);
        }

        [Fact]
        public async Task GetRestaurantListFromFilter_RetuenRestaurantCardListModel_WhenRequestByVisitor()
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

            // arrange mock req
            var mockRequest = new GetRestaurantListFromFilterRequestModel()
            {
                latitude = 13.736717,
                longitude = 100.523186,
                skip = 0,
                take = 20,
                Keywords = "Test",
                IsOpen = true,
                CategoryType = new List<int>() { 1, 2 },
                DeliveryType = new List<int>() { 1, 2 },
                PaymentMethod = new List<int>() { 1, 2 }
            };

            // setup dapper response
            mockDapperUnitOfWork.Setup(x => x.KinMaiRepository.QueryAsync<RestaurantCardInfoModel>(It.IsAny<string>()))
                                .Returns(() => Task.FromResult<IEnumerable<RestaurantCardInfoModel>>(mockRestaurantCard));

            // act
            var actualOutput = await reviewerService.GetRestaurantListFromFilter(mockRequest);
            var expectOutput = new RestaurantCardListModel()
            {
                RestaurantInfo = mockRestaurantCard,
                TotalRestaurant = 1,
                RestaurantCumulativeCount = 1
            };

            // assert
            mockDapperUnitOfWork.VerifyAll();
            var actualOutputObj = JsonConvert.SerializeObject(actualOutput);
            var expetedOutputObj = JsonConvert.SerializeObject(expectOutput);
            Assert.Equal(expetedOutputObj, actualOutputObj);
        }

        [Fact]
        public async Task GetRestaurantListFromFilter_ThrowNullReferenceException_WhenRequestWithInValidModel()
        {
            // arrange mock req
            var mockRequest = new GetRestaurantListFromFilterRequestModel()
            {
                userId = Guid.NewGuid(),
                latitude = 0,
                longitude = 0,
            };

            // act
            Func<Task> actualOutput = () => reviewerService.GetRestaurantListFromFilter(mockRequest);

            // assert
            var exception = await Assert.ThrowsAsync<NullReferenceException>(actualOutput);
            Assert.Equal("Object reference not set to an instance of an object.", exception.Message);
        }
    }
}

