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
    public class GetReviewInfo
    {
        private readonly InitConfiguration initConfiguration;
        private readonly Mock<IDapperUnitOfWork> mockDapperUnitOfWork;
        private readonly Mock<IS3UnitOfWork> mockS3UnitOfWork;
        private readonly Mock<IEntityUnitOfWork> mockEntityUnitOfWork;
        private readonly IReviewerService reviewerService;

        public GetReviewInfo()
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
        public async Task GetReviewInfo_ReturnReviewInfoModel_WhenRequestWithValidModel()
        {
            // arrange
            var mockReview = new Review()
            {
                Id = Guid.NewGuid(),
                RestaurantId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Rating = 5,
                Comment = "Good taste",
                FoodRecommendList = new List<string>() { "Pepsi" }.ToArray(),
                ReviewLabelRecommend = new List<int>() { 1 }.ToArray(),
                CreateAt = DateTime.UtcNow,
            };

            // setup query response
            mockEntityUnitOfWork.Setup(x => x.ReviewRepository.GetSingleAsync(It.IsAny<Expression<Func<Review, bool>>>()))
                                .ReturnsAsync(() => mockReview);

            var mockRequest = new GetReviewInfoRequest()
            {
                UserId = Guid.NewGuid(),
                RestaurantId = Guid.NewGuid(),
            };

            // act
            var actualOutput = await reviewerService.GetReviewInfo(mockRequest);
            var expectedOutput = new ReviewInfoModel()
            {
                ReviewId = mockReview.Id,
                Comment = mockReview.Comment,
                Rating = mockReview.Rating,
                FoodRecommendList = mockReview.FoodRecommendList.ToList(),
                ImageLink = new List<string>(),
                ReviewLabelList = mockReview.ReviewLabelRecommend.ToList(),
            };

            // assert
            mockEntityUnitOfWork.VerifyAll();
            var actualOutputObj = JsonConvert.SerializeObject(actualOutput);
            var expetedOutputObj = JsonConvert.SerializeObject(expectedOutput);
            Assert.Equal(expetedOutputObj, actualOutputObj);
        }

        [Fact]
        public async Task GetReviewInfo_ThrowArgumentException_WhenReviewDoesNotExist()
        {
            // setup query response
            mockEntityUnitOfWork.Setup(x => x.ReviewRepository.GetSingleAsync(It.IsAny<Expression<Func<Review, bool>>>()))
                                .ReturnsAsync(() => null);

            // arrange
            var mockRequest = new GetReviewInfoRequest()
            {
                UserId = Guid.NewGuid(),
                RestaurantId = Guid.NewGuid(),
            };

            // act
            Func<Task> actualOutput = () => reviewerService.GetReviewInfo(mockRequest);

            // assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(actualOutput);
            mockEntityUnitOfWork.VerifyAll();
            Assert.Equal("This review does not exists.", exception.Message);
        }

        [Fact]
        public async Task GetReviewInfo_ThrowNullReferenceException_WhenRequestWithInValidModel()
        {
            // arrange
            var mockRequest = new GetReviewInfoRequest()
            {
                UserId = Guid.NewGuid(),
            };

            // act
            Func<Task> actualOutput = () => reviewerService.GetReviewInfo(mockRequest);

            // assert
            var exception = await Assert.ThrowsAsync<NullReferenceException>(actualOutput);
            Assert.Equal("Object reference not set to an instance of an object.", exception.Message);
        }
    }
}
