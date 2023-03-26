using KinMai.Api.Models.Reviewer;
using KinMai.Common.Enum;
using KinMai.Dapper.Interface;
using KinMai.EntityFramework.Models;
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
    public class GetRestaurantNearMeList
    {
        private readonly InitConfiguration initConfiguration;
        private readonly Mock<IDapperUnitOfWork> mockDapperUnitOfWork;
        private readonly Mock<IS3UnitOfWork> mockS3UnitOfWork;
        private readonly Mock<IEntityUnitOfWork> mockEntityUnitOfWork;
        private readonly IReviewerService reviewerService;

        public GetRestaurantNearMeList()
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
        public async Task GetRestaurantNearMeList_RetuenRestaurantInfoListModel_WhenRequestWithValidModel()
        {
            var mockRestaurantInfo = new List<RestaurantInfoItemModel>() {
                new RestaurantInfoItemModel()
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
                    ImageCover = "testImageCover",
                    AnotherImageCover = new List<string>(),
                    IsFavorite = false,
                    isReview = false
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
            var mockRequest = new GetRestaurantNearMeRequestModel()
            {
                userId = new Guid("9c16fe15-f21e-4071-94e8-c982b6c9c626"), //nampunch1@gmail.com
                latitude = 13.736717,
                longitude = 100.523186,
                skip = 0, //**
                take = 20  //**
            };

            // setup db & dapper response
            mockEntityUnitOfWork.Setup(x => x.RestaurantRepository.GetAll())
                                .Returns(() => queryableRestaurant);
            mockDapperUnitOfWork.Setup(x => x.KinMaiRepository.QueryAsync<RestaurantInfoItemModel>(It.IsAny<string>()))
                                .Returns(() => Task.FromResult<IEnumerable<RestaurantInfoItemModel>>(mockRestaurantInfo));

            // act
            var actualOutput = await reviewerService.GetRestaurantNearMeList(mockRequest);
            var expectOutput = new RestaurantInfoListModel()
            {
                RestaurantInfo = mockRestaurantInfo,
                TotalRestaurant = 1,
                RestaurantCumulativeCount = 1
            };

            // assert
            var actualOutputObj = JsonConvert.SerializeObject(actualOutput);
            var expetedOutputObj = JsonConvert.SerializeObject(expectOutput);
            mockEntityUnitOfWork.VerifyAll();
            mockDapperUnitOfWork.VerifyAll();
            Assert.Equal(expetedOutputObj, actualOutputObj);
        }

        [Fact]
        public async Task GetRestaurantNearMeList_RetuenRestaurantInfoListModel_WhenUserDoesNotExist()
        {
            var mockRestaurantInfo = new List<RestaurantInfoItemModel>() {
                new RestaurantInfoItemModel()
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
                    ImageCover = "testImageCover",
                    AnotherImageCover = new List<string>(),
                    IsFavorite = false,
                    isReview = false
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
            var mockRequest = new GetRestaurantNearMeRequestModel()
            {
                latitude = 13.736717,
                longitude = 100.523186,
                skip = 0,
                take = 20
            };

            // setup db & dapper response
            mockEntityUnitOfWork.Setup(x => x.RestaurantRepository.GetAll())
                                .Returns(() => queryableRestaurant);
            mockDapperUnitOfWork.Setup(x => x.KinMaiRepository.QueryAsync<RestaurantInfoItemModel>(It.IsAny<string>()))
                                .Returns(() => Task.FromResult<IEnumerable<RestaurantInfoItemModel>>(mockRestaurantInfo));

            // act
            var actualOutput = await reviewerService.GetRestaurantNearMeList(mockRequest);
            var expectOutput = new RestaurantInfoListModel()
            {
                RestaurantInfo = mockRestaurantInfo,
                TotalRestaurant = 1,
                RestaurantCumulativeCount = 1
            };

            // assert
            var actualOutputObj = JsonConvert.SerializeObject(actualOutput);
            var expetedOutputObj = JsonConvert.SerializeObject(expectOutput);
            mockEntityUnitOfWork.VerifyAll();
            mockDapperUnitOfWork.VerifyAll();
            Assert.Equal(expetedOutputObj, actualOutputObj);
        }

        [Fact]
        public async Task GetRestaurantNearMeList_ThrowNullReferenceException_WhenRequestWithInvalidModel()
        {
            // arrange mock req
            var mockRequest = new GetRestaurantNearMeRequestModel()
            {
                userId = Guid.NewGuid(),
                latitude = 0,
                longitude = 0,
            };

            // act
            Func<Task> actualOutput = () => reviewerService.GetRestaurantNearMeList(mockRequest);

            // assert
            var exception = await Assert.ThrowsAsync<NullReferenceException>(actualOutput);
            Assert.Equal("Object reference not set to an instance of an object.", exception.Message);
        }
    }
}
