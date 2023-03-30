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

namespace KinMai.UnitTests.Services.RestaurantServiceTest
{
    public class GetAllReviews
    {
        private readonly InitConfiguration initConfiguration;
        private readonly Mock<IDapperUnitOfWork> mockDapperUnitOfWork;
        private readonly Mock<IS3UnitOfWork> mockS3UnitOfWork;
        private readonly Mock<IEntityUnitOfWork> mockEntityUnitOfWork;
        private readonly IRestaurantService restaurantService;

        public GetAllReviews()
        {
            initConfiguration = new InitConfiguration();
            mockDapperUnitOfWork = new Mock<IDapperUnitOfWork>();
            mockS3UnitOfWork = new Mock<IS3UnitOfWork>();
            mockEntityUnitOfWork = new Mock<IEntityUnitOfWork>();
            restaurantService = new RestaurantService(
                mockEntityUnitOfWork.Object,
                mockDapperUnitOfWork.Object,
                mockS3UnitOfWork.Object
            );
        }

        [Fact]
        public async Task GetAllReviews_ReturnListReviewInfoModel_WhenRestaurantHaveReview()
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

            var mockReviews = new List<Review>()
            {
                new Review()
                {
                    Id= Guid.NewGuid(),
                    RestaurantId = mockRestaurant.Id,
                    UserId = Guid.NewGuid(),
                    Rating = 5,
                    Comment = "Good taste",
                    FoodRecommendList = new List<string>() { "Pepsi" }.ToArray(),
                    CreateAt = DateTime.UtcNow,
                }
            };

            var mockReviewInfoResponse = new List<ReviewInfoModel>()
            {
                new ReviewInfoModel()
                {
                    ReviewId = mockReviews[0].Id,
                    Rating = mockReviews[0].Rating,
                    Comment = mockReviews[0].Comment,
                    CreateAt = mockReviews[0].CreateAt,
                    FoodRecommendList = mockReviews[0].FoodRecommendList.ToList(),
                    ImageLink = new List<string>() {},
                    ReplyComment = "",
                    ReviewLabelList = new List<int>() {},
                    UserId = mockReviews[0].UserId,
                    UserName = "littlepuncchz"
                }
            };

            IQueryable<Review> mockReviewsQueryable = mockReviews.AsQueryable();

            // setup db response
            mockEntityUnitOfWork.Setup(x => x.RestaurantRepository.GetSingle(It.IsAny<Expression<Func<Restaurant, bool>>>()))
                                .Returns(() => mockRestaurant);
            mockEntityUnitOfWork.Setup(x => x.ReviewRepository.GetAll(It.IsAny<Expression<Func<Review, bool>>>()))
                                .Returns(() => mockReviewsQueryable);

            // setup dapper response
            mockDapperUnitOfWork.Setup(x => x.KinMaiRepository.QueryAsync<ReviewInfoModel>(It.IsAny<string>()))
                                .Returns(() => Task.FromResult<IEnumerable<ReviewInfoModel>>(mockReviewInfoResponse));

            // act
            var actualOutput = await restaurantService.GetAllReviews(mockRestaurant.Id);
            var expectedOutput = new ListReviewInfoModel()
            {
                reviews = mockReviewInfoResponse
            };

            // assert
            var actualOutputObj = JsonConvert.SerializeObject(actualOutput);
            var expetedOutputObj = JsonConvert.SerializeObject(expectedOutput);
            mockEntityUnitOfWork.VerifyAll();
            mockDapperUnitOfWork.VerifyAll();
            Assert.Equal(expetedOutputObj, actualOutputObj);
        }

        [Fact]
        public async Task GetAllReviews_ThrowArgumentException_WhenRestaurantDoesNotExist()
        {
            // setup db response
            mockEntityUnitOfWork.Setup(x => x.RestaurantRepository.GetSingle(It.IsAny<Expression<Func<Restaurant, bool>>>()))
                                .Returns(() => null);

            // act
            Func<Task> actualOutput = () => restaurantService.GetAllReviews(Guid.NewGuid());

            // assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(actualOutput);
            mockEntityUnitOfWork.VerifyAll();
            Assert.Equal("This restaurant does not exists.", exception.Message);
        }

        [Fact]
        public async Task GetAllReviews_ThrowArgumentException_WhenRestaurantDidNotHaveReview()
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

            // setup db response
            mockEntityUnitOfWork.Setup(x => x.RestaurantRepository.GetSingle(It.IsAny<Expression<Func<Restaurant, bool>>>()))
                                .Returns(() => mockRestaurant);
            mockEntityUnitOfWork.Setup(x => x.ReviewRepository.GetAll(It.IsAny<Expression<Func<Review, bool>>>()))
                                .Returns(() => null);

            // act
            Func<Task> actualOutput = () => restaurantService.GetAllReviews(mockRestaurant.Id);

            // assert
            var exception = await Assert.ThrowsAsync<Exception>(actualOutput);
            mockEntityUnitOfWork.VerifyAll();
            Assert.Equal("This reviews does not exists.", exception.Message);
        }
    }
}
