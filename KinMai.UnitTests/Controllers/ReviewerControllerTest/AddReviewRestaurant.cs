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
using KinMai.S3.Models;
using KinMai.S3.UnitOfWork.Interface;
using KinMai.UnitTests.Shared;
using Microsoft.AspNetCore.Http;
using Moq;
using Newtonsoft.Json;
using System.Linq.Expressions;

namespace KinMai.UnitTests.Controllers.ReviewerControllerTest
{
    public class AddReviewRestaurant
    {
        private readonly InitConfiguration initConfiguration;
        private readonly Mock<IDapperUnitOfWork> mockDapperUnitOfWork;
        private readonly Mock<IS3UnitOfWork> mockS3UnitOfWork;
        private readonly Mock<IMailUnitOfWork> mockMailUnitOfWork;
        private readonly Mock<IEntityUnitOfWork> mockEntityUnitOfWork;
        private readonly Mock<IAuthenticationUnitOfWork> mockAuthenticationUnitOfWork;
        private readonly ILogicUnitOfWork logicUnitOfWork;
        private readonly ReviewerController reviewerController;

        public AddReviewRestaurant()
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
        public async Task AddReviewRestaurant_ReturnStatus200_WhenRequestWithValidModel()
        {
            // arrange
            var mockUser = new User()
            {
                Id = Guid.NewGuid(),
                FirstName = "Supansa",
                LastName = "Tantulset",
                Email = "test@test.com",
                Username = "littlepunchhz",
                CreateAt = DateTime.UtcNow,
                UserType = 1,
                IsLoginWithGoogle = false
            };

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

            // mock image file
            byte[] filebytes = File.ReadAllBytes($"{AppDomain.CurrentDomain.BaseDirectory}/Images/restaurant.jpg");
            IFormFile imageFileMock = new FormFile(new MemoryStream(filebytes), 0, filebytes.Length, "Data", "restaurant.png");

            // setup query response
            mockEntityUnitOfWork.Setup(x => x.UserRepository.GetSingleAsync(It.IsAny<Expression<Func<User, bool>>>()))
                                .ReturnsAsync(() => mockUser);
            mockEntityUnitOfWork.Setup(x => x.RestaurantRepository.GetSingleAsync(It.IsAny<Expression<Func<Restaurant, bool>>>()))
                                .ReturnsAsync(() => mockRestaurant);
            mockEntityUnitOfWork.Setup(x => x.ReviewRepository.GetSingleAsync(It.IsAny<Expression<Func<Review, bool>>>()))
                                .ReturnsAsync(() => null);
            mockEntityUnitOfWork.Setup(x => x.ReviewRepository.Add(It.IsAny<Review>()));
            mockEntityUnitOfWork.Setup(x => x.SaveAsync());

            // setup s3 response
            mockS3UnitOfWork.Setup(x => x.S3FileService.UploadImage(It.IsAny<UploadImageModel>()))
                            .Returns(Task.FromResult("image link"));

            var mockRequest = new AddReviewRequestModel()
            {
                UserId = Guid.NewGuid(),
                RestaurantId = Guid.NewGuid(),
                Comment = "Good Taste",
                Rating = 5,
                FoodRecommendList = new List<string>() { "Coffee" },
                ReviewLabelList = new List<int>() { 1, 2 },
                ImageFiles = new List<IFormFile>() { imageFileMock }
            };

            // act
            var actualOutput = await reviewerController.AddReviewRestaurant(mockRequest);
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
        public async Task AddReviewRestaurant_ReturnStatus400_WhenReviewIsAlreadyExist()
        {
            // arrange
            var mockUser = new User()
            {
                Id = Guid.NewGuid(),
                FirstName = "Supansa",
                LastName = "Tantulset",
                Email = "test@test.com",
                Username = "littlepunchhz",
                CreateAt = DateTime.UtcNow,
                UserType = 1,
                IsLoginWithGoogle = false
            };

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

            // mock image file
            byte[] filebytes = File.ReadAllBytes($"{AppDomain.CurrentDomain.BaseDirectory}/Images/restaurant.jpg");
            IFormFile imageFileMock = new FormFile(new MemoryStream(filebytes), 0, filebytes.Length, "Data", "restaurant.png");

            // setup query response
            mockEntityUnitOfWork.Setup(x => x.UserRepository.GetSingleAsync(It.IsAny<Expression<Func<User, bool>>>()))
                                .ReturnsAsync(() => mockUser);
            mockEntityUnitOfWork.Setup(x => x.RestaurantRepository.GetSingleAsync(It.IsAny<Expression<Func<Restaurant, bool>>>()))
                                .ReturnsAsync(() => mockRestaurant);
            mockEntityUnitOfWork.Setup(x => x.ReviewRepository.GetSingleAsync(It.IsAny<Expression<Func<Review, bool>>>()))
                                .ReturnsAsync(() => mockReview);

            var mockRequest = new AddReviewRequestModel()
            {
                UserId = Guid.NewGuid(),
                RestaurantId = Guid.NewGuid(),
                Comment = "Good Taste",
                Rating = 5,
                FoodRecommendList = new List<string>() { "Coffee" },
                ReviewLabelList = new List<int>() { 1, 2 },
                ImageFiles = new List<IFormFile>() { imageFileMock }
            };

            var actualOutput = await reviewerController.AddReviewRestaurant(mockRequest);
            var expectedOutput = new ResponseModel<bool>()
            {
                Data = false,
                Status = 400,
                Message = "You already review this restaurant."
            };

            // assert
            var actualOutputObj = JsonConvert.SerializeObject(actualOutput);
            var expetedOutputObj = JsonConvert.SerializeObject(expectedOutput);
            Assert.Equal(expetedOutputObj, actualOutputObj);
        }

        [Fact]
        public async Task AddReviewRestaurant_ReturnStatus400_WhenRestaurantDoesNotExist()
        {
            // arrange
            var mockUser = new User()
            {
                Id = Guid.NewGuid(),
                FirstName = "Supansa",
                LastName = "Tantulset",
                Email = "test@test.com",
                Username = "littlepunchhz",
                CreateAt = DateTime.UtcNow,
                UserType = 1,
                IsLoginWithGoogle = false
            };

            // mock image file
            byte[] filebytes = File.ReadAllBytes($"{AppDomain.CurrentDomain.BaseDirectory}/Images/restaurant.jpg");
            IFormFile imageFileMock = new FormFile(new MemoryStream(filebytes), 0, filebytes.Length, "Data", "restaurant.png");

            // setup query response
            mockEntityUnitOfWork.Setup(x => x.UserRepository.GetSingleAsync(It.IsAny<Expression<Func<User, bool>>>()))
                                .ReturnsAsync(() => mockUser);
            mockEntityUnitOfWork.Setup(x => x.RestaurantRepository.GetSingleAsync(It.IsAny<Expression<Func<Restaurant, bool>>>()))
                                .ReturnsAsync(() => null);
            mockEntityUnitOfWork.Setup(x => x.ReviewRepository.GetSingleAsync(It.IsAny<Expression<Func<Review, bool>>>()))
                                .ReturnsAsync(() => null);

            var mockRequest = new AddReviewRequestModel()
            {
                UserId = Guid.NewGuid(),
                RestaurantId = Guid.NewGuid(),
                Comment = "Good Taste",
                Rating = 5,
                FoodRecommendList = new List<string>() { "Coffee" },
                ReviewLabelList = new List<int>() { 1, 2 },
                ImageFiles = new List<IFormFile>() { imageFileMock }
            };

            var actualOutput = await reviewerController.AddReviewRestaurant(mockRequest);
            var expectedOutput = new ResponseModel<bool>()
            {
                Data = false,
                Status = 400,
                Message = "This restaurant is not exists."
            };

            // assert
            var actualOutputObj = JsonConvert.SerializeObject(actualOutput);
            var expetedOutputObj = JsonConvert.SerializeObject(expectedOutput);
            Assert.Equal(expetedOutputObj, actualOutputObj);
        }

        [Fact]
        public async Task AddReviewRestaurant_ReturnStatus400_WhenUserDoesNotExist()
        {
            // arrange
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

            // mock image file
            byte[] filebytes = File.ReadAllBytes($"{AppDomain.CurrentDomain.BaseDirectory}/Images/restaurant.jpg");
            IFormFile imageFileMock = new FormFile(new MemoryStream(filebytes), 0, filebytes.Length, "Data", "restaurant.png");

            // setup query response
            mockEntityUnitOfWork.Setup(x => x.UserRepository.GetSingleAsync(It.IsAny<Expression<Func<User, bool>>>()))
                                .ReturnsAsync(() => null);
            mockEntityUnitOfWork.Setup(x => x.RestaurantRepository.GetSingleAsync(It.IsAny<Expression<Func<Restaurant, bool>>>()))
                                .ReturnsAsync(() => mockRestaurant);
            mockEntityUnitOfWork.Setup(x => x.ReviewRepository.GetSingleAsync(It.IsAny<Expression<Func<Review, bool>>>()))
                                .ReturnsAsync(() => null);

            var mockRequest = new AddReviewRequestModel()
            {
                UserId = Guid.NewGuid(),
                RestaurantId = Guid.NewGuid(),
                Comment = "Good Taste",
                Rating = 5,
                FoodRecommendList = new List<string>() { "Coffee" },
                ReviewLabelList = new List<int>() { 1, 2 },
                ImageFiles = new List<IFormFile>() { imageFileMock }
            };

            var actualOutput = await reviewerController.AddReviewRestaurant(mockRequest);
            var expectedOutput = new ResponseModel<bool>()
            {
                Data = false,
                Status = 400,
                Message = "This user is not exists."
            };

            // assert
            var actualOutputObj = JsonConvert.SerializeObject(actualOutput);
            var expetedOutputObj = JsonConvert.SerializeObject(expectedOutput);
            Assert.Equal(expetedOutputObj, actualOutputObj);
        }
    }
}
