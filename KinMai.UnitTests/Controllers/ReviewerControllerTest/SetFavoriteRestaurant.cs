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

namespace KinMai.UnitTests.Controllers.ReviewerControllerTest
{
    public class SetFavoriteRestaurant
    {
        private readonly InitConfiguration initConfiguration;
        private readonly Mock<IDapperUnitOfWork> mockDapperUnitOfWork;
        private readonly Mock<IS3UnitOfWork> mockS3UnitOfWork;
        private readonly Mock<IMailUnitOfWork> mockMailUnitOfWork;
        private readonly Mock<IEntityUnitOfWork> mockEntityUnitOfWork;
        private readonly Mock<IAuthenticationUnitOfWork> mockAuthenticationUnitOfWork;
        private readonly ILogicUnitOfWork logicUnitOfWork;
        private readonly ReviewerController reviewerController;

        public SetFavoriteRestaurant()
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
        public async Task SetFavoriteRestaurant_ReturnStatus200_WhenRequestFavoriteRestaurant()
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

            mockEntityUnitOfWork.Setup(x => x.UserRepository.GetSingle(It.IsAny<Expression<Func<User, bool>>>()))
                                .Returns(() => mockUser);
            mockEntityUnitOfWork.Setup(x => x.RestaurantRepository.GetSingle(It.IsAny<Expression<Func<Restaurant, bool>>>()))
                                .Returns(() => mockRestaurant);
            mockEntityUnitOfWork.Setup(x => x.FavoriteRestaurantRepository.GetSingleAsync(It.IsAny<Expression<Func<FavoriteRestaurant, bool>>>()))
                                .ReturnsAsync(() => null);
            mockEntityUnitOfWork.Setup(x => x.FavoriteRestaurantRepository.Add(It.IsAny<FavoriteRestaurant>()));
            mockEntityUnitOfWork.Setup(x => x.SaveAsync());

            var mockRequest = new SetFavoriteResturantRequestModel()
            {
                UserId = Guid.NewGuid(),
                RestaurantId = Guid.NewGuid(),
                IsFavorite = true
            };

            // act
            var actualOutput = await reviewerController.SetFavoriteRestaurant(mockRequest);
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
        public async Task SetFavoriteRestaurant_ReturnStatus200_WhenRequestUnFavoriteRestaurant()
        {
            // arrange
            var mockFavoriteRestaurantItem = new FavoriteRestaurant()
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                RestaurantId = Guid.NewGuid(),
                CreateAt = DateTime.UtcNow
            };

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

            mockEntityUnitOfWork.Setup(x => x.UserRepository.GetSingle(It.IsAny<Expression<Func<User, bool>>>()))
                                .Returns(() => mockUser);
            mockEntityUnitOfWork.Setup(x => x.RestaurantRepository.GetSingle(It.IsAny<Expression<Func<Restaurant, bool>>>()))
                                .Returns(() => mockRestaurant);
            mockEntityUnitOfWork.Setup(x => x.FavoriteRestaurantRepository.GetSingleAsync(It.IsAny<Expression<Func<FavoriteRestaurant, bool>>>()))
                                .ReturnsAsync(() => mockFavoriteRestaurantItem);
            mockEntityUnitOfWork.Setup(x => x.FavoriteRestaurantRepository.Delete(It.IsAny<FavoriteRestaurant>()));
            mockEntityUnitOfWork.Setup(x => x.SaveAsync());

            var mockRequest = new SetFavoriteResturantRequestModel()
            {
                UserId = mockFavoriteRestaurantItem.UserId,
                RestaurantId = mockFavoriteRestaurantItem.RestaurantId,
                IsFavorite = false
            };

            // act
            var actualOutput = await reviewerController.SetFavoriteRestaurant(mockRequest);
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
        public async Task SetFavoriteRestaurant_ReturnStatus400_WhenUserDoesNotExist()
        {
            // arrange
            var mockRequest = new SetFavoriteResturantRequestModel()
            {
                UserId = Guid.NewGuid(),
                RestaurantId = Guid.NewGuid(),
                IsFavorite = false,
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

            mockEntityUnitOfWork.Setup(x => x.UserRepository.GetSingle(It.IsAny<Expression<Func<User, bool>>>()))
                                .Returns(() => null);
            mockEntityUnitOfWork.Setup(x => x.RestaurantRepository.GetSingle(It.IsAny<Expression<Func<Restaurant, bool>>>()))
                                .Returns(() => mockRestaurant);

            // act
            var actualOutput = await reviewerController.SetFavoriteRestaurant(mockRequest);
            var expectedOutput = new ResponseModel<bool>()
            {
                Data = false,
                Status = 400,
                Message = "User does not exist."
            };

            // assert
            var actualOutputObj = JsonConvert.SerializeObject(actualOutput);
            var expetedOutputObj = JsonConvert.SerializeObject(expectedOutput);
            Assert.Equal(expetedOutputObj, actualOutputObj);
        }

        [Fact]
        public async Task SetFavoriteRestaurant_ReturnStatus400_WhenRestaurantDoesNotExist()
        {
            // arrange
            var mockRequest = new SetFavoriteResturantRequestModel()
            {
                UserId = Guid.NewGuid(),
                RestaurantId = Guid.NewGuid(),
                IsFavorite = false,
            };

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

            mockEntityUnitOfWork.Setup(x => x.UserRepository.GetSingle(It.IsAny<Expression<Func<User, bool>>>()))
                                .Returns(() => mockUser);
            mockEntityUnitOfWork.Setup(x => x.RestaurantRepository.GetSingle(It.IsAny<Expression<Func<Restaurant, bool>>>()))
                                .Returns(() => null);

            /// act
            var actualOutput = await reviewerController.SetFavoriteRestaurant(mockRequest);
            var expectedOutput = new ResponseModel<bool>()
            {
                Data = false,
                Status = 400,
                Message = "Restaurant does not exist."
            };

            // assert
            var actualOutputObj = JsonConvert.SerializeObject(actualOutput);
            var expetedOutputObj = JsonConvert.SerializeObject(expectedOutput);
            Assert.Equal(expetedOutputObj, actualOutputObj);
        }
    }
}
