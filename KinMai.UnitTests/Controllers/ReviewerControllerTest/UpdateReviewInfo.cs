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
using KinMai.S3.Models;
using KinMai.S3.UnitOfWork.Interface;
using KinMai.UnitTests.Shared;
using Microsoft.AspNetCore.Http;
using Moq;
using Newtonsoft.Json;
using System.Linq.Expressions;

namespace KinMai.UnitTests.Controllers.ReviewerControllerTest
{
    public class UpdateReviewInfo
    {
        private readonly InitConfiguration initConfiguration;
        private readonly Mock<IDapperUnitOfWork> mockDapperUnitOfWork;
        private readonly Mock<IS3UnitOfWork> mockS3UnitOfWork;
        private readonly Mock<IMailUnitOfWork> mockMailUnitOfWork;
        private readonly Mock<IEntityUnitOfWork> mockEntityUnitOfWork;
        private readonly Mock<IAuthenticationUnitOfWork> mockAuthenticationUnitOfWork;
        private readonly ILogicUnitOfWork logicUnitOfWork;
        private readonly ReviewerController reviewerController;

        public UpdateReviewInfo()
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
        public async Task UpdateReviewInfo_ReturnStatus200_WhenRequestWithValidModel()
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
                RemoveImageLink = new List<string>() { "imageLink " }
            };

            // act
            var actualOutput = await reviewerController.UpdateReviewInfo(mockRequest);
            var expectedOutput = new ResponseModel<bool>()
            {
                Data = true,
                Status = 200,
                Message = "success"
            };

            // assert
            var actualOutputObj = JsonConvert.SerializeObject(actualOutput);
            var expetedOutputObj = JsonConvert.SerializeObject(expectedOutput);
            Assert.Equal(expetedOutputObj, actualOutputObj);
        }

        [Fact]
        public async Task UpdateReviewInfo_ReturnStatus400_WhenReviewDoesNotExist()
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
            var actualOutput = await reviewerController.UpdateReviewInfo(mockRequest);
            var expectedOutput = new ResponseModel<bool>()
            {
                Data = false,
                Status = 400,
                Message = "This review does not exists."
            };

            // assert
            var actualOutputObj = JsonConvert.SerializeObject(actualOutput);
            var expetedOutputObj = JsonConvert.SerializeObject(expectedOutput);
            Assert.Equal(expetedOutputObj, actualOutputObj);
        }

        [Fact]
        public async Task UpdateReviewInfo_ReturnStatus500_WhenRequestWithInValidModel()
        {
            // arrange
            var mockRequest = new UpdateReviewInfoRequest()
            {
                Rating = 4,
                FoodRecommendList = new List<string>() { "Somtam" },
                RemoveImageLink = new List<string>() { "imageLink " }
            };

            // act
            var actualOutput = await reviewerController.UpdateReviewInfo(mockRequest);
            var expectedOutput = new ResponseModel<bool>()
            {
                Data = false,
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
