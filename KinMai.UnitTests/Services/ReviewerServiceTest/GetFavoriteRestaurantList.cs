using System;
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
	public class GetFavoriteRestaurantList
	{
        private readonly InitConfiguration initConfiguration;
        private readonly Mock<IDapperUnitOfWork> mockDapperUnitOfWork;
        private readonly Mock<IS3UnitOfWork> mockS3UnitOfWork;
        private readonly Mock<IEntityUnitOfWork> mockEntityUnitOfWork;
        private readonly IReviewerService reviewerService;

        public GetFavoriteRestaurantList()
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
        public async Task GetFavoriteRestaurantList_ReturnGetFavoriteRestaurantList_WhenRequestWithValidModel()
        {
            // arrange
            var mockUser = new User()
            {
                Id = Guid.NewGuid(),
                FirstName = "Supansa",
                LastName = "Tantulset",
                Username = "littlepunchhz",
                Email = "test@test.com",
                CreateAt = DateTime.UtcNow,
                UserType = 1,
                IsLoginWithGoogle = false
            };

            var mockGetFavoriteRestaurantList = new List<KinMai.Logic.Models.GetFavoriteRestaurantList>()
            {
                new KinMai.Logic.Models.GetFavoriteRestaurantList
                {
                    RestaurantId = Guid.NewGuid(),
                    RestaurantName = "Somtam",
                    ImageCover = "imageLink",
                    MinPriceRate = 100,
                    MaxPriceRate = 200,
                    Description = "Good Taste, Good Mood",
                    TotalReview = 10,
                    IsOpen = true,
                    Rating = 5,
                    Distance = 100
                }
            };

            // setup query response
            mockEntityUnitOfWork.Setup(x => x.UserRepository.GetSingleAsync(It.IsAny<Expression<Func<User, bool>>>()))
                                .ReturnsAsync(() => mockUser);

            // setup dapper response
            mockDapperUnitOfWork.Setup(x => x.KinMaiRepository.QueryAsync<KinMai.Logic.Models.GetFavoriteRestaurantList>(It.IsAny<string>()))
                                .Returns(() => Task.FromResult<IEnumerable<KinMai.Logic.Models.GetFavoriteRestaurantList>>(mockGetFavoriteRestaurantList));

            var mockRequest = new GetFavoriteRestaurantRequest()
            {
                UserId = Guid.NewGuid(),
                Latitude = 100,
                Longitude = 200
            };

            // act
            var actualOutput = await reviewerService.GetFavoriteRestaurantList(mockRequest);
            var expectedOutput = mockGetFavoriteRestaurantList;

            // assert
            mockEntityUnitOfWork.VerifyAll();
            var actualOutputObj = JsonConvert.SerializeObject(actualOutput);
            var expetedOutputObj = JsonConvert.SerializeObject(expectedOutput);
            Assert.Equal(expetedOutputObj, actualOutputObj);
        }

        [Fact]
        public async Task GetFavoriteRestaurantList_ThrowArgumentException_WhenUserDoesNotExist()
        {
            // setup query response
            mockEntityUnitOfWork.Setup(x => x.UserRepository.GetSingleAsync(It.IsAny<Expression<Func<User, bool>>>()))
                                .ReturnsAsync(() => null);

            // arrange
            var mockRequest = new GetFavoriteRestaurantRequest()
            {
                UserId = Guid.NewGuid(),
                Latitude = 100,
                Longitude = 200
            };

            // act
            Func<Task> actualOutput = () => reviewerService.GetFavoriteRestaurantList(mockRequest);

            // assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(actualOutput);
            mockEntityUnitOfWork.VerifyAll();
            Assert.Equal("User does not exists.", exception.Message);
        }

        [Fact]
        public async Task GetFavoriteRestaurantList_ThrowNullReferenceException_WhenRequestWithInvalidModel()
        {
            // arrange
            var mockRequest = new GetFavoriteRestaurantRequest()
            {
                UserId = Guid.NewGuid(),
                Longitude = 200
            };

            // act
            Func<Task> actualOutput = () => reviewerService.GetFavoriteRestaurantList(mockRequest);

            // assert
            var exception = await Assert.ThrowsAsync<NullReferenceException>(actualOutput);
            Assert.Equal("Object reference not set to an instance of an object.", exception.Message);
        }
    }
}

