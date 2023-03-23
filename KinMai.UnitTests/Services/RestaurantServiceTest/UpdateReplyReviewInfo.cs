using KinMai.Dapper.Interface;
using KinMai.EntityFramework.Models;
using KinMai.EntityFramework.UnitOfWork.Interface;
using KinMai.Logic.Interface;
using KinMai.Logic.Models;
using KinMai.Logic.Services;
using KinMai.S3.UnitOfWork.Interface;
using KinMai.UnitTests.Shared;
using Moq;
using System.Linq.Expressions;

namespace KinMai.UnitTests.Services.RestaurantServiceTest
{
    public class UpdateReplyReviewInfo
    {
        private readonly InitConfiguration initConfiguration;
        private readonly Mock<IDapperUnitOfWork> mockDapperUnitOfWork;
        private readonly Mock<IS3UnitOfWork> mockS3UnitOfWork;
        private readonly Mock<IEntityUnitOfWork> mockEntityUnitOfWork;
        private readonly IRestaurantService restaurantService;

        public UpdateReplyReviewInfo()
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
        public async Task UpdateReplyReviewInfo_ReturnTrue_WhenReviewIsExist()
        {
            var mockReview = new Review()
            {
                Id = Guid.NewGuid(),
                RestaurantId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Rating = 5,
                Comment = "Good taste",
                FoodRecommendList = new List<string>() { "Pepsi" }.ToArray(),
                CreateAt = DateTime.UtcNow,
            };

            var mockRequest = new UpdateReplyReviewInfoRequest()
            {
                ReviewId = mockReview.Id,
                ReplyComment = "Thanks"
            };

            // setup db response
            mockEntityUnitOfWork.Setup(x => x.ReviewRepository.GetSingleAsync(It.IsAny<Expression<Func<Review, bool>>>()))
                                .ReturnsAsync(() => mockReview);
            mockEntityUnitOfWork.Setup(x => x.ReviewRepository.Update(It.IsAny<Review>()));
            mockEntityUnitOfWork.Setup(x => x.SaveAsync());

            // act
            var actualOutput = await restaurantService.UpdateReplyReviewInfo(mockRequest);

            // assert
            mockEntityUnitOfWork.VerifyAll();
            Assert.True(actualOutput);
        }

        [Fact]
        public async Task UpdateReplyReviewInfo_ThrowArgumentException_WhenReviewDoesNotExist()
        {
            var mockRequest = new UpdateReplyReviewInfoRequest()
            {
                ReviewId = Guid.NewGuid(),
                ReplyComment = "Thanks"
            };

            // setup db response
            mockEntityUnitOfWork.Setup(x => x.ReviewRepository.GetSingleAsync(It.IsAny<Expression<Func<Review, bool>>>()))
                                .ReturnsAsync(() => null);

            // act
            Func<Task> actualOutput = () => restaurantService.UpdateReplyReviewInfo(mockRequest);

            // assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(actualOutput);
            mockEntityUnitOfWork.VerifyAll();
            Assert.Equal("This review does not exists.", exception.Message);
        }
    }
}
