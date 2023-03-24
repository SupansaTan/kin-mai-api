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
    public class GetRestaurantDetail
    {
        private readonly InitConfiguration initConfiguration;
        private readonly Mock<IDapperUnitOfWork> mockDapperUnitOfWork;
        private readonly Mock<IS3UnitOfWork> mockS3UnitOfWork;
        private readonly Mock<IEntityUnitOfWork> mockEntityUnitOfWork;
        private readonly IReviewerService reviewerService;

        public GetRestaurantDetail()
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
        public async Task GetRestaurantDetail_ReturnGetRestaurantDetailModel_WhenRequestWithValidModel()
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
            var actualOutput = await reviewerService.GetRestaurantDetail(mockRequest);
            var expectedOutput = mockRestaurantInfo[0];

            // assert
            mockEntityUnitOfWork.VerifyAll();
            mockDapperUnitOfWork.VerifyAll();
            var actualOutputObj = JsonConvert.SerializeObject(actualOutput);
            var expetedOutputObj = JsonConvert.SerializeObject(expectedOutput);
            Assert.Equal(expetedOutputObj, actualOutputObj);
        }

        [Fact]
        public async Task GetRestaurantDetail_ReturnGetRestaurantDetailModel_WhenRequestByVisitor()
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
            var actualOutput = await reviewerService.GetRestaurantDetail(mockRequest);
            var expectedOutput = mockRestaurantInfo[0];

            // assert
            mockEntityUnitOfWork.VerifyAll();
            mockDapperUnitOfWork.VerifyAll();
            var actualOutputObj = JsonConvert.SerializeObject(actualOutput);
            var expetedOutputObj = JsonConvert.SerializeObject(expectedOutput);
            Assert.Equal(expetedOutputObj, actualOutputObj);
        }

        [Fact]
        public async Task GetRestaurantDetail_ThrowArgumentException_WhenRestaurantDoesNotExist()
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
            Func<Task> actualOutput = () => reviewerService.GetRestaurantDetail(mockRequest);

            // assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(actualOutput);
            mockEntityUnitOfWork.VerifyAll();
            Assert.Equal("Restaurant does not exist.", exception.Message);
        }

        [Fact]
        public async Task GetRestaurantDetail_ThrowNullReferenceException_WhenRequestWithInValidModel()
        {
            // arrange
            var mockRequest = new GetRestaurantDetailRequestModel()
            {
                UserId = Guid.NewGuid(),
                Latitude = 0,
                Longitude = 0,
            };

            // act
            Func<Task> actualOutput = () => reviewerService.GetRestaurantDetail(mockRequest);

            // assert
            var exception = await Assert.ThrowsAsync<NullReferenceException>(actualOutput);
            Assert.Equal("Object reference not set to an instance of an object.", exception.Message);
        }
    }
}
