using KinMai.Api.Controllers;
using KinMai.Api.Models;
using KinMai.Authentication.UnitOfWork;
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

namespace KinMai.UnitTests.Controllers.ReviewerControllerTest
{
    public class GetReviewInfo
    {
        private readonly InitConfiguration initConfiguration;
        private readonly Mock<IDapperUnitOfWork> mockDapperUnitOfWork;
        private readonly Mock<IS3UnitOfWork> mockS3UnitOfWork;
        private readonly Mock<IMailUnitOfWork> mockMailUnitOfWork;
        private readonly Mock<IEntityUnitOfWork> mockEntityUnitOfWork;
        private readonly Mock<IAuthenticationUnitOfWork> mockAuthenticationUnitOfWork;
        private readonly ILogicUnitOfWork logicUnitOfWork;
        private readonly ReviewerController reviewerController;

        public GetReviewInfo()
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
        public async Task GetReviewInfo_ReturnStatus200_WhenRequestWithValidModel()
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
            // act
            var actualOutput = await reviewerController.GetReviewInfo(mockRequest);
            var expectedOutput = new ResponseModel<ReviewInfoModel>()
            {
                Data = new ReviewInfoModel()
                {
                    ReviewId = mockReview.Id,
                    Comment = mockReview.Comment,
                    Rating = mockReview.Rating,
                    FoodRecommendList = mockReview.FoodRecommendList.ToList(),
                    ImageLink = new List<string>(),
                    ReviewLabelList = mockReview.ReviewLabelRecommend.ToList(),
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
        public async Task GetReviewInfo_ReturnStatus400_WhenReviewDoesNotExist()
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
            var actualOutput = await reviewerController.GetReviewInfo(mockRequest);
            var expectedOutput = new ResponseModel<ReviewInfoModel>()
            {
                Data = null,
                Status = 400,
                Message = "This review does not exists."
            };

            // assert
            var actualOutputObj = JsonConvert.SerializeObject(actualOutput);
            var expetedOutputObj = JsonConvert.SerializeObject(expectedOutput);
            Assert.Equal(expetedOutputObj, actualOutputObj);
        }

        [Fact]
        public async Task GetReviewInfo_ReturnStatus500_WhenRequestWithInValidModel()
        {
            // arrange
            var mockRequest = new GetReviewInfoRequest()
            {
                UserId = Guid.NewGuid(),
            };

            // act
            var actualOutput = await reviewerController.GetReviewInfo(mockRequest);
            var expectedOutput = new ResponseModel<ReviewInfoModel>()
            {
                Data = null,
                Status = 500,
                Message = "Object reference not set to an instance of an object."
            };

            // assert
            var actualOutputObj = JsonConvert.SerializeObject(actualOutput);
            var expetedOutputObj = JsonConvert.SerializeObject(expectedOutput);
            Assert.Equal(expetedOutputObj, actualOutputObj);
        }
    }
}
