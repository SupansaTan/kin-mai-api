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
using Xunit.Abstractions;

namespace KinMai.UnitTests.Controllers.ReviewerControllerTest
{
	public class GetRestaurantNearMeList
	{
        private readonly ITestOutputHelper output;
        private readonly InitConfiguration initConfiguration;
        private readonly Mock<IDapperUnitOfWork> mockDapperUnitOfWork;
        private readonly Mock<IS3UnitOfWork> mockS3UnitOfWork;
        private readonly Mock<IMailUnitOfWork> mockMailUnitOfWork;
        private readonly Mock<IEntityUnitOfWork> mockEntityUnitOfWork;
        private readonly IAuthenticationUnitOfWork mockAuthenticationUnitOfWork;
        public GetRestaurantNearMeList(ITestOutputHelper output)
		{
            this.output = output;
            initConfiguration = new InitConfiguration();
            mockDapperUnitOfWork = new Mock<IDapperUnitOfWork>();
            mockS3UnitOfWork = new Mock<IS3UnitOfWork>();
            mockMailUnitOfWork = new Mock<IMailUnitOfWork>();
            mockEntityUnitOfWork = new Mock<IEntityUnitOfWork>();
            mockAuthenticationUnitOfWork = new AuthenticationUnitOfWork();
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

            // mock dapper & controller
            mockEntityUnitOfWork.Setup(x => x.RestaurantRepository.GetAll())
                                .Returns(() => queryableRestaurant);
            mockDapperUnitOfWork.Setup(x => x.KinMaiRepository.QueryAsync<RestaurantInfoItemModel>(It.IsAny<string>()))
                                .Returns(() => Task.FromResult<IEnumerable<RestaurantInfoItemModel>>(mockRestaurantInfo));

            ILogicUnitOfWork logicUnitOfWork = new LogicUnitOfWork(
                mockEntityUnitOfWork.Object,
                mockDapperUnitOfWork.Object,
                mockAuthenticationUnitOfWork,
                mockS3UnitOfWork.Object,
                mockMailUnitOfWork.Object
            );
            var reviewerController = new ReviewerController(logicUnitOfWork);

            // arrange mock req
            var mockRequest = new GetRestaurantNearMeRequestModel()
            {
                userId = new Guid("9c16fe15-f21e-4071-94e8-c982b6c9c626"), //nampunch1@gmail.com
                latitude = 13.736717,
                longitude = 100.523186,
                skip = 0, //**
                take = 20  //**
            };

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
            Assert.Equal(expectOutput.Data.RestaurantInfo, actualOutput.Data.RestaurantInfo);
            Assert.Equal(expectOutput.Data.TotalRestaurant, actualOutput.Data.TotalRestaurant);
            Assert.Equal(expectOutput.Data.RestaurantCumulativeCount, actualOutput.Data.RestaurantCumulativeCount);
            Assert.Equal(expectOutput.Message, actualOutput.Message);
            Assert.Equal(expectOutput.Status, actualOutput.Status);
        }

        [Fact]
        public async Task GetRestaurantNearMeList_RetuenStatus400_WhenResquestInvalid()
        {
           // arrange mock req
            var mockRequest = new GetRestaurantNearMeRequestModel()
            {
                userId = new Guid(" "), 
                latitude = 0,
                longitude = 0,
                skip = 0, //**
                take = 0  //**
            };

            // ** mock dapper & controller 
            mockEntityUnitOfWork.Setup(x => x.RestaurantRepository.GetAll()).Returns(() => It.IsAny<IQueryable<Restaurant>>());
            mockDapperUnitOfWork.Setup(x => x.KinMaiRepository.QueryAsync<RestaurantInfoItemModel>(It.IsAny<string>())).Returns(() => Task.FromResult<IEnumerable<RestaurantInfoItemModel>>(mockRestaurantInfo));

            ILogicUnitOfWork logicUnitOfWork = new LogicUnitOfWork(
                mockEntityUnitOfWork.Object,
                mockDapperUnitOfWork.Object,
                mockAuthenticationUnitOfWork,
                mockS3UnitOfWork.Object,
                mockMailUnitOfWork.Object
            );
            var reviewerController = new ReviewerController(logicUnitOfWork);

            // act
            var actualOutput = await reviewerController.GetRestaurantNearMeList(mockRequest);
            output.WriteLine(actualOutput.ToString());
            var expectOutput = new ResponseModel<RestaurantInfoListModel>
            {
                Data = null,
                Message = "Can't find restaurant near me",
                Status = 400
            };

            // assert
            Assert.Equal(expectOutput.Data, actualOutput.Data);
            Assert.Equal(expectOutput.Message, actualOutput.Message);
            Assert.Equal(expectOutput.Status, actualOutput.Status);

        }

        //500 - External error เช่น error ที่ service ของ amazon ที่เราไม่ได้ดักไว้, และก็เคสที่เราไม่ได้ดักไว้ก็จะ throw Exception ออกมา ขึ้น 500 เหมือนกัน
        [Fact]
        public async Task GetRestaurantNearMeList_RetuenStatus500_WhenRestaurantNotExist()
        {
            //add restaurant info
            var mockRestaurantInfo = new List<RestaurantInfoItemModel>() {
                new RestaurantInfoItemModel()
                {
                    RestaurantId = Guid.NewGuid(),
                    RestaurantName = "Test",
                    Rating = 0,
                    StartTime = " ",
                    EndTime = " ",
                    Distance = 0,
                    MinPriceRate = 0,
                    MaxPriceRate = 0,
                    TotalReview = 0,
                    ImageCover = "testImageCover",
                    AnotherImageCover = new List<string>(),
                    IsFavorite = false,
                    isReview = false
                }
            };

            // ** mock dapper & controller 
            mockEntityUnitOfWork.Setup(x => x.RestaurantRepository.GetAll()).Returns(() => It.IsAny<IQueryable<Restaurant>>());
            mockDapperUnitOfWork.Setup(x => x.KinMaiRepository.QueryAsync<RestaurantInfoItemModel>(It.IsAny<string>())).Returns(() => Task.FromResult<IEnumerable<RestaurantInfoItemModel>>(mockRestaurantInfo));

            ILogicUnitOfWork logicUnitOfWork = new LogicUnitOfWork(
                mockEntityUnitOfWork.Object,
                mockDapperUnitOfWork.Object,
                mockAuthenticationUnitOfWork,
                mockS3UnitOfWork.Object,
                mockMailUnitOfWork.Object
            );
            var reviewerController = new ReviewerController(logicUnitOfWork);

            // arrange mock req
            var mockRequest = new GetRestaurantNearMeRequestModel()
            {
                userId = new Guid("9xxxxxxxxxxxxxxxx"), //nampunch1@gmail.com
                latitude = 0,
                longitude = 0,
                skip = 0, //**
                take = 0  //**
            };

            // act
            var actualOutput = await reviewerController.GetRestaurantNearMeList(mockRequest);
            output.WriteLine(actualOutput.ToString());
            var expectOutput = new ResponseModel<RestaurantInfoListModel>
            {
                Data = null,
                Message = "Restaurant doesn't exist",
                Status = 500
            };

            // assert
            Assert.Equal(expectOutput.Data, actualOutput.Data);
            Assert.Equal(expectOutput.Message, actualOutput.Message);
            Assert.Equal(expectOutput.Status, actualOutput.Status);

        }
    }
}

