using KinMai.Dapper.Interface;
using KinMai.EntityFramework.Models;
using KinMai.EntityFramework.UnitOfWork.Interface;
using KinMai.Logic.Interface;
using KinMai.Logic.Models;
using KinMai.Logic.Services;
using KinMai.S3.Models;
using KinMai.S3.UnitOfWork.Interface;
using KinMai.UnitTests.Shared;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Linq.Expressions;

namespace KinMai.UnitTests.Services.ReviewerServiceTest
{
    public class UpdateReviewInfo
    {
        private readonly InitConfiguration initConfiguration;
        private readonly Mock<IDapperUnitOfWork> mockDapperUnitOfWork;
        private readonly Mock<IS3UnitOfWork> mockS3UnitOfWork;
        private readonly Mock<IEntityUnitOfWork> mockEntityUnitOfWork;
        private readonly IReviewerService reviewerService;

        public UpdateReviewInfo()
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
        public async Task UpdateReviewInfo_ReturnTrue_WhenRequestWithValidModel()
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
                ImageLink = new List<string>() { "imageLink" }.ToArray(),
                CreateAt = DateTime.UtcNow,
            };

            // mock image file
            byte[] filebytes = File.ReadAllBytes($"{AppDomain.CurrentDomain.BaseDirectory}/Images/restaurant.jpg");
            IFormFile imageFileMock = new FormFile(new MemoryStream(filebytes), 0, filebytes.Length, "Data", "restaurant.png");

            // setup query response
            mockEntityUnitOfWork.Setup(x => x.ReviewRepository.GetSingleAsync(It.IsAny<Expression<Func<Review, bool>>>()))
                                .ReturnsAsync(() => mockReview);
            mockEntityUnitOfWork.Setup(x => x.ReviewRepository.Update(It.IsAny<Review>()));
            mockEntityUnitOfWork.Setup(x => x.SaveAsync());

            // setup s3 response
            mockS3UnitOfWork.Setup(x => x.S3FileService.UploadImage(It.IsAny<UploadImageModel>()))
                            .Returns(Task.FromResult("image link"));
            mockS3UnitOfWork.Setup(x => x.S3FileService.DeleteFile(It.IsAny<string>(), It.IsAny<string>()));

            var mockRequest = new UpdateReviewInfoRequest()
            {
                ReviewId = Guid.NewGuid(),
                Rating = 4,
                FoodRecommendList = new List<string>() { "Somtam" },
                NewImageFile = new List<IFormFile>() { imageFileMock },
                RemoveImageLink = new List<string>() { "imageLink "}
            };

            // act
            var actualOutput = await reviewerService.UpdateReviewInfo(mockRequest);

            // assert
            mockEntityUnitOfWork.VerifyAll();
            mockS3UnitOfWork.VerifyAll();
            Assert.True(actualOutput);
        }

        [Fact]
        public async Task UpdateReviewInfo_ThrowArgumentException_WhenReviewDoesNotExist()
        {
            // mock image file
            byte[] filebytes = File.ReadAllBytes($"{AppDomain.CurrentDomain.BaseDirectory}/Images/restaurant.jpg");
            IFormFile imageFileMock = new FormFile(new MemoryStream(filebytes), 0, filebytes.Length, "Data", "restaurant.png");

            // setup query response
            mockEntityUnitOfWork.Setup(x => x.ReviewRepository.GetSingleAsync(It.IsAny<Expression<Func<Review, bool>>>()))
                                .ReturnsAsync(() => null);

            // arrange
            var mockRequest = new UpdateReviewInfoRequest()
            {
                ReviewId = Guid.NewGuid(),
                Rating = 4,
                FoodRecommendList = new List<string>() { "Somtam" },
                NewImageFile = new List<IFormFile>() { imageFileMock },
                RemoveImageLink = new List<string>() { "imageLink " }
            };

            // act
            Func<Task> actualOutput = () => reviewerService.UpdateReviewInfo(mockRequest);

            // assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(actualOutput);
            mockEntityUnitOfWork.VerifyAll();
            Assert.Equal("This review does not exists.", exception.Message);
        }

        [Fact]
        public async Task UpdateReviewInfo_ThrowNullReferenceException_WhenRequestWithInValidModel()
        {
            // arrange
            var mockRequest = new UpdateReviewInfoRequest()
            {
                Rating = 4,
                FoodRecommendList = new List<string>() { "Somtam" },
                RemoveImageLink = new List<string>() { "imageLink " }
            };

            // act
            Func<Task> actualOutput = () => reviewerService.UpdateReviewInfo(mockRequest);

            // assert
            var exception = await Assert.ThrowsAsync<NullReferenceException>(actualOutput);
            Assert.Equal("Object reference not set to an instance of an object.", exception.Message);
        }
    }
}
