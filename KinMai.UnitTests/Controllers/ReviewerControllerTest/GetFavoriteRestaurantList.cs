using System;
using System.Linq.Expressions;
using KinMai.Api.Controllers;
using KinMai.Api.Models;
using KinMai.Authentication.UnitOfWork;
using KinMai.Dapper.Interface;
using KinMai.EntityFramework.Models;
using KinMai.EntityFramework.UnitOfWork.Interface;
using KinMai.Logic.Models;
using KinMai.Logic.Services;
using KinMai.Logic.UnitOfWork.Implement;
using KinMai.Logic.UnitOfWork.Interface;
using KinMai.Mail.UnitOfWork;
using KinMai.S3.UnitOfWork.Interface;
using KinMai.UnitTests.Shared;
using Moq;
using Newtonsoft.Json;

namespace KinMai.UnitTests.Controllers.ReviewerControllerTest
{
	public class GetFavoriteRestaurantList
	{
        private readonly InitConfiguration initConfiguration;
        private readonly Mock<IDapperUnitOfWork> mockDapperUnitOfWork;
        private readonly Mock<IS3UnitOfWork> mockS3UnitOfWork;
        private readonly Mock<IMailUnitOfWork> mockMailUnitOfWork;
        private readonly Mock<IEntityUnitOfWork> mockEntityUnitOfWork;
        private readonly Mock<IAuthenticationUnitOfWork> mockAuthenticationUnitOfWork;
        private readonly ILogicUnitOfWork logicUnitOfWork;
        private readonly ReviewerController reviewerController;

        public GetFavoriteRestaurantList()
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
        public async Task GetFavoriteRestaurantList_ReturnStatus200_WhenRequestWithValidModel()
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
            var actualOutput = await reviewerController.GetFavoriteRestaurantList(mockRequest);
            var expectedOutput = new ResponseModel<List<KinMai.Logic.Models.GetFavoriteRestaurantList>>()
            {
                Data = mockGetFavoriteRestaurantList,
                Status = 200,
                Message = "success"
            };

            // assert
            var actualOutputObj = JsonConvert.SerializeObject(actualOutput);
            var expetedOutputObj = JsonConvert.SerializeObject(expectedOutput);
            Assert.Equal(expetedOutputObj, actualOutputObj);
        }

        [Fact]
        public async Task GetFavoriteRestaurantList_ReturnStatus400_WhenUserDoesNotExist()
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
            var actualOutput = await reviewerController.GetFavoriteRestaurantList(mockRequest);
            var expectedOutput = new ResponseModel<List<KinMai.Logic.Models.GetFavoriteRestaurantList>>()
            {
                Data = null,
                Status = 400,
                Message = "User does not exists."
            };

            // assert
            var actualOutputObj = JsonConvert.SerializeObject(actualOutput);
            var expetedOutputObj = JsonConvert.SerializeObject(expectedOutput);
            Assert.Equal(expetedOutputObj, actualOutputObj);
        }

        [Fact]
        public async Task GetFavoriteRestaurantList_ReturnStatus500_WhenRequestWithInvalidModel()
        {
            // arrange
            var mockRequest = new GetFavoriteRestaurantRequest()
            {
                UserId = Guid.NewGuid(),
                Longitude = 200
            };

            // act
            var actualOutput = await reviewerController.GetFavoriteRestaurantList(mockRequest);
            var expectedOutput = new ResponseModel<List<KinMai.Logic.Models.GetFavoriteRestaurantList>>()
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

