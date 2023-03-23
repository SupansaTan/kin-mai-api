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

namespace KinMai.UnitTests.Controllers.RestaurantControllerTest
{
    public class GetRestaurantReviews
    {
        private readonly InitConfiguration initConfiguration;
        private readonly Mock<IDapperUnitOfWork> mockDapperUnitOfWork;
        private readonly Mock<IS3UnitOfWork> mockS3UnitOfWork;
        private readonly Mock<IMailUnitOfWork> mockMailUnitOfWork;
        private readonly Mock<IAuthenticationUnitOfWork> mockAuthenticationUnitOfWork;
        private readonly Mock<IEntityUnitOfWork> mockEntityUnitOfWork;
        private readonly ILogicUnitOfWork logicUnitOfWork;
        private readonly RestaurantController restaurantController;

        public GetRestaurantReviews()
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
        public async Task GetRestaurantReviews_ReturnStatus200_WhenRestaurantHaveReview()
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
            var actualOutput = await restaurantController.GetRestaurantReviews(mockRestaurant.Id);
            var expectedOutput = new ResponseModel<ListReviewInfoModel>()
            {
                Data = new ListReviewInfoModel()
                {
                    reviews = mockReviewInfoResponse
                },
                Status = 200,
                Message = "success"
            };

            // assert
            var actualOutputObj = JsonConvert.SerializeObject(actualOutput);
            var expetedOutputObj = JsonConvert.SerializeObject(expectedOutput);
            Assert.Equal(expetedOutputObj, actualOutputObj);
        }

        [Fact]
        public async Task GetRestaurantReviews_ReturnStatus400_WhenRestaurantDoesNotExist()
        {
            // setup db response
            mockEntityUnitOfWork.Setup(x => x.RestaurantRepository.GetSingle(It.IsAny<Expression<Func<Restaurant, bool>>>()))
                                .Returns(() => null);

            // act
            var actualOutput = await restaurantController.GetRestaurantReviews(Guid.NewGuid());
            var expectedOutput = new ResponseModel<ListReviewInfoModel>()
            {
                Data = null,
                Status = 400,
                Message = "This restaurant does not exists."
            };

            // assert
            var actualOutputString = JsonConvert.SerializeObject(actualOutput);
            var expetedOutputString = JsonConvert.SerializeObject(expectedOutput);
            Assert.Equal(expetedOutputString, actualOutputString);
        }

        [Fact]
        public async Task GetRestaurantReviews_ReturnStatus500_WhenRestaurantDidNotHaveReview()
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
            var actualOutput = await restaurantController.GetRestaurantReviews(mockRestaurant.Id);
            var expectedOutput = new ResponseModel<ListReviewInfoModel>()
            {
                Data = null,
                Status = 500,
                Message = "This reviews does not exists."
            };

            // assert
            var actualOutputString = JsonConvert.SerializeObject(actualOutput);
            var expetedOutputString = JsonConvert.SerializeObject(expectedOutput);
            Assert.Equal(expetedOutputString, actualOutputString);
        }
    }
}
