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
using System.Linq.Expressions;

namespace KinMai.UnitTests.Services.ReviewerServiceTest
{
    public class GetRestaurantReviewList
    {
        private readonly InitConfiguration initConfiguration;
        private readonly Mock<IDapperUnitOfWork> mockDapperUnitOfWork;
        private readonly Mock<IS3UnitOfWork> mockS3UnitOfWork;
        private readonly Mock<IEntityUnitOfWork> mockEntityUnitOfWork;
        private readonly IReviewerService reviewerService;

        public GetRestaurantReviewList()
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
        public async Task GetRestaurantReviewList_ReturnGetReviewInfoListModel_WhenRequestWithValidModel()
        {
            // arrange
            var mockReviewInfo = new List<GetReviewInfoModel>() {
                new GetReviewInfoModel()
                {
                    Comment = "Good taste",
                    Rating = 5,
                    FoodRecommendList = new List<string>() { "Pepsi" },
                    ImageReviewList = new List<string>() { "iamgeLink" },
                    ReviewLabelList = new List<int>() { 1 },
                    Username = "littlepunchhz",
                    CreatedDateDiff = 200,
                    RestaurantReply = "Thanks"
                }
            };

            var mockTotalReview = new List<GetTotalReviewModel>()
            {
                new GetTotalReviewModel()
                {
                    TotalReview = 1,
                    TotalReviewHaveComment = 1,
                    TotalReviewHaveFoodRecommend = 1,
                    TotalReviewHaveImage = 1
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
                ImageLink = new List<string>() { "iamgeLink" }.ToArray(),
                DeliveryType = new List<int>() { 1 }.ToArray(),
                PaymentMethod = new List<int>() { 1 }.ToArray(),
                Description = "Good Taste, Good mood",
                Latitude = 0,
                Longitude = 0,
                CreateAt = DateTime.Now
            };

            // setup query & dapper response
            mockEntityUnitOfWork.Setup(x => x.RestaurantRepository.GetSingleAsync(It.IsAny<Expression<Func<Restaurant, bool>>>()))
                                .ReturnsAsync(() => mockRestaurant);
            mockDapperUnitOfWork.Setup(x => x.KinMaiRepository.QueryAsync<GetReviewInfoModel>(It.IsAny<string>()))
                                .Returns(() => Task.FromResult<IEnumerable<GetReviewInfoModel>>(mockReviewInfo));
            mockDapperUnitOfWork.Setup(x => x.KinMaiRepository.QueryAsync<GetTotalReviewModel>(It.IsAny<string>()))
                               .Returns(() => Task.FromResult<IEnumerable<GetTotalReviewModel>>(mockTotalReview));

            var mockRequest = new GetReviewInfoFilterModel()
            {
                RestaurantId = Guid.NewGuid(),
                Keywords = "test",
                Rating = 6,
                IsOnlyReviewHaveImage = true,
                IsOnlyReviewHaveComment = false,
                IsOnlyReviewHaveFoodRecommend = false
            };

            // act
            var actualOutput = await reviewerService.GetRestaurantReviewList(mockRequest);
            var expectedOutput = new GetReviewInfoListModel()
            {
                ReviewList = mockReviewInfo,
                TotalReview = mockTotalReview[0].TotalReview,
                TotalReviewHaveImage = mockTotalReview[0].TotalReviewHaveImage,
                TotalReviewHaveComment = mockTotalReview[0].TotalReviewHaveComment,
                TotalReviewHaveFoodRecommend = mockTotalReview[0].TotalReviewHaveFoodRecommend
            };

            // assert
            mockEntityUnitOfWork.VerifyAll();
            mockDapperUnitOfWork.VerifyAll();
            var actualOutputObj = JsonConvert.SerializeObject(actualOutput);
            var expetedOutputObj = JsonConvert.SerializeObject(expectedOutput);
            Assert.Equal(expetedOutputObj, actualOutputObj);
        }

        [Fact]
        public async Task GetRestaurantReviewList_ThrowArgumentException_WhenRestaurantDoesNotExist()
        {
            // setup query & dapper response
            mockEntityUnitOfWork.Setup(x => x.RestaurantRepository.GetSingleAsync(It.IsAny<Expression<Func<Restaurant, bool>>>()))
                                .ReturnsAsync(() => null);

            // arrange
            var mockRequest = new GetReviewInfoFilterModel()
            {
                RestaurantId = Guid.NewGuid(),
                Keywords = "test",
                Rating = 6,
                IsOnlyReviewHaveImage = true,
                IsOnlyReviewHaveComment = false,
                IsOnlyReviewHaveFoodRecommend = false
            };

            // act
            Func<Task> actualOutput = () => reviewerService.GetRestaurantReviewList(mockRequest);

            // assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(actualOutput);
            mockEntityUnitOfWork.VerifyAll();
            Assert.Equal("Restaurant does not exist.", exception.Message);
        }

        [Fact]
        public async Task GetRestaurantReviewList_ThrowNullReferenceException_WhenRequestWithInValidModel()
        {
            // arrange
            var mockRequest = new GetReviewInfoFilterModel()
            {
                Keywords = "test",
                Rating = 6,
                IsOnlyReviewHaveComment = false,
                IsOnlyReviewHaveFoodRecommend = false
            };

            // act
            Func<Task> actualOutput = () => reviewerService.GetRestaurantReviewList(mockRequest);

            // assert
            var exception = await Assert.ThrowsAsync<NullReferenceException>(actualOutput);
            Assert.Equal("Object reference not set to an instance of an object.", exception.Message);
        }
    }
}
