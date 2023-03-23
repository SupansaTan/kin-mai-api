using KinMai.Api.Controllers;
using KinMai.Api.Models;
using KinMai.Api.Models.Reviewer;
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
	public class GetRestaurantNearMeList
	{
        private readonly InitConfiguration initConfiguration;
        private readonly Mock<IDapperUnitOfWork> mockDapperUnitOfWork;
        private readonly Mock<IS3UnitOfWork> mockS3UnitOfWork;
        private readonly Mock<IMailUnitOfWork> mockMailUnitOfWork;
        private readonly Mock<IEntityUnitOfWork> mockEntityUnitOfWork;
        private readonly Mock<IAuthenticationUnitOfWork> mockAuthenticationUnitOfWork;
        private readonly ILogicUnitOfWork logicUnitOfWork;
        private readonly ReviewerController reviewerController;

        public GetRestaurantNearMeList()
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
		public async Task GetRestaurantNearMeList_RetuenStatus200_WhenRequestWithValidModel()
		{
            var mockRestaurantInfo = new List<RestaurantInfoItemModel>() {
                new RestaurantInfoItemModel()
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
                    ImageCover = "testImageCover",
                    AnotherImageCover = new List<string>(),
                    IsFavorite = false,
                    isReview = false
                }
            };

            var mockRestaurant = new List<Restaurant>()
            {
                new Restaurant()
                {
                    Id = Guid.NewGuid(),
                    OwnerId = Guid.NewGuid(),
                    Name = "Test",
                    RestaurantType = (int)RestaurantType.All,
                    MinPriceRate = 200,
                    MaxPriceRate = 1000,
                    Latitude = 0,
                    Longitude = 0,
                    CreateAt = DateTime.Now
                }
            };
            IQueryable<Restaurant> queryableRestaurant = mockRestaurant.AsQueryable();

            // arrange mock req
            var mockRequest = new GetRestaurantNearMeRequestModel()
            {
                userId = new Guid("9c16fe15-f21e-4071-94e8-c982b6c9c626"), //nampunch1@gmail.com
                latitude = 13.736717,
                longitude = 100.523186,
                skip = 0, //**
                take = 20  //**
            };

            var mockUser = new User()
            {
                Id = mockRequest.userId,
                FirstName = "Supansa",
                LastName = "Tantulset",
                Username = "littlepunchhz",
                UserType = 1,
                CreateAt = DateTime.UtcNow,
                Email = "test@gmail.com",
                IsLoginWithGoogle = false
            };

            // setup db & dapper response
            mockEntityUnitOfWork.Setup(x => x.UserRepository.GetSingle(It.IsAny<Expression<Func<User,bool>>>()))
                                .Returns(() => mockUser);
            mockEntityUnitOfWork.Setup(x => x.RestaurantRepository.GetAll())
                                .Returns(() => queryableRestaurant);
            mockDapperUnitOfWork.Setup(x => x.KinMaiRepository.QueryAsync<RestaurantInfoItemModel>(It.IsAny<string>()))
                                .Returns(() => Task.FromResult<IEnumerable<RestaurantInfoItemModel>>(mockRestaurantInfo));

            // act
            var actualOutput = await reviewerController.GetRestaurantNearMeList(mockRequest);
            var expectOutput = new ResponseModel<RestaurantInfoListModel>
            {
                Data = new RestaurantInfoListModel()
                {
                    RestaurantInfo = mockRestaurantInfo,
                    TotalRestaurant = 1,
                    RestaurantCumulativeCount = 1
                },
                Message = "success",
                Status = 200
            };

            // assert
            var actualOutputObj = JsonConvert.SerializeObject(actualOutput);
            var expetedOutputObj = JsonConvert.SerializeObject(expectOutput);
            Assert.Equal(expetedOutputObj, actualOutputObj);
        }

        [Fact]
        public async Task GetRestaurantNearMeList_RetuenStatus400_WhenUserDoesNotExist()
        {
            // mock dapper & controller
            mockEntityUnitOfWork.Setup(x => x.UserRepository.GetSingle(It.IsAny<Expression<Func<User, bool>>>()))
                                .Returns(() => null);

            // arrange mock req
            var mockRequest = new GetRestaurantNearMeRequestModel()
            {
                userId = Guid.NewGuid(),
                latitude = 13.736717,
                longitude = 100.523186,
                skip = 0,
                take = 20
            };

            // act
            var actualOutput = await reviewerController.GetRestaurantNearMeList(mockRequest);
            var expectOutput = new ResponseModel<RestaurantInfoListModel>
            {
                Data = null,
                Message = "User does not exist.",
                Status = 400
            };

            // assert
            var actualOutputObj = JsonConvert.SerializeObject(actualOutput);
            var expetedOutputObj = JsonConvert.SerializeObject(expectOutput);
            Assert.Equal(expetedOutputObj, actualOutputObj);
        }

        [Fact]
        public async Task GetRestaurantNearMeList_RetuenStatus500_WhenRequestWithInvalidModel()
        {
            // arrange mock req
            var mockRequest = new GetRestaurantNearMeRequestModel()
            {
                userId = Guid.NewGuid(), 
                latitude = 0,
                longitude = 0,
            };

            // act
            var actualOutput = await reviewerController.GetRestaurantNearMeList(mockRequest);
            var expectOutput = new ResponseModel<RestaurantInfoListModel>
            {
                Data = null,
                Message = "Object reference not set to an instance of an object.",
                Status = 500
            };

            // assert
            var actualOutputObj = JsonConvert.SerializeObject(actualOutput);
            var expetedOutputObj = JsonConvert.SerializeObject(expectOutput);
            Assert.Equal(expetedOutputObj, actualOutputObj);
        }
    }
}

