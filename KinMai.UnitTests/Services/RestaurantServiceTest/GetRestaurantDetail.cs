using KinMai.Common.Enum;
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
using Xunit.Abstractions;

namespace KinMai.UnitTests.Services.RestaurantServiceTest
{
    public class GetRestaurantDetail
    {
        private readonly ITestOutputHelper output;
        private readonly InitConfiguration initConfiguration;
        private readonly Mock<IDapperUnitOfWork> mockDapperUnitOfWork;
        private readonly Mock<IS3UnitOfWork> mockS3UnitOfWork;
        private readonly Mock<IEntityUnitOfWork> mockEntityUnitOfWork;
        private readonly IRestaurantService restaurantService;

        public GetRestaurantDetail(ITestOutputHelper output)
        {
            this.output = output;
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
        public async Task GetRestaurantDetail_ReturnRestaurantDetailModel_WhenRestaurantIsExist()
        {
            var mockUser = new User()
            {
                Id = Guid.NewGuid(),
                Email = "nampunch1@gmail.com",
                FirstName = "Supansa",
                LastName = "Tantulset",
                Username = "littlepunchhz",
                CreateAt = DateTime.UtcNow,
                UserType = 1,
                IsLoginWithGoogle = false
            };

            var mockRestaurant = new Restaurant()
            {
                Id = Guid.NewGuid(),
                OwnerId = mockUser.Id,
                Name = "Somtam",
                Description = "Good taste, Good mood",
                Address = "1111 Pracharat Rd.",
                CreateAt = DateTime.UtcNow,
                RestaurantType = (int)RestaurantType.All,
                Latitude = 100,
                Longitude = 36,
                MinPriceRate = 100,
                MaxPriceRate = 300,
                Owner = mockUser,
                ImageLink = new List<string>() { "imageLink" }.ToArray(),
                DeliveryType = new List<int>() { 1, 2 }.ToArray(),
                PaymentMethod = new List<int>() { 1, 2 }.ToArray()
            };

            var mockSocialContact = new List<SocialContact>()
            {
                new SocialContact()
                {
                    Id = Guid.NewGuid(),
                    RestaurantId = mockRestaurant.Id,
                    SocialType = (int)SocialContactType.Tel,
                    ContactValue = "0888888888"
                }
            };

            var mockCategory = new List<Category>()
            {
                new Category()
                {
                    Id = Guid.NewGuid(),
                    Name = "Cafe",
                    Type = 12,
                }
            };

            var mockRelated = new List<Related>()
            {
                new Related()
                {
                    Id = Guid.NewGuid(),
                    RestaurantId = mockRestaurant.Id,
                    CategoriesId = mockCategory[0].Id,
                    Restaurant = mockRestaurant
                }
            };

            var mockBusinessHour = new List<string>()
            {
                JsonConvert.SerializeObject(
                    new ResBusinessHourModel()
                    {
                        Day = 1,
                        OpenTime = TimeOnly.FromDateTime(DateTime.UtcNow).AddHours(7).ToString(),
                        CloseTime = TimeOnly.FromDateTime(DateTime.UtcNow).AddHours(7).ToString()
                    }
                )
            };

            var mockRestaurantInfoResponse = new List<ResArrayDataModel>()
            {
                new ResArrayDataModel()
                {
                    ImageLink = mockRestaurant.ImageLink,
                    DeliveryType = mockRestaurant.DeliveryType,
                    PaymentMethod = mockRestaurant.PaymentMethod,
                    BusinessHour = mockBusinessHour
                }
            };

            IQueryable<SocialContact> mockSocialContactQueryable = mockSocialContact.AsQueryable();
            IQueryable<Category> mockCategoryQueryable = mockCategory.AsQueryable();
            IQueryable<Related> mockRelatedQueryable = mockRelated.AsQueryable();

            // setup db response
            mockEntityUnitOfWork.Setup(x => x.RestaurantRepository.GetSingleAsync(It.IsAny<Expression<Func<Restaurant, bool>>>()))
                                .Returns(() => Task.FromResult(mockRestaurant));
            mockEntityUnitOfWork.Setup(x => x.SocialContactRepository.GetAll(It.IsAny<Expression<Func<SocialContact, bool>>>()))
                                .Returns(() => mockSocialContactQueryable);
            mockEntityUnitOfWork.Setup(x => x.CategoryRepository.GetAll())
                                .Returns(() => mockCategoryQueryable);
            mockEntityUnitOfWork.Setup(x => x.RelatedRepository.GetAll(It.IsAny<Expression<Func<Related, bool>>>()))
                                .Returns(() => mockRelatedQueryable);

            // setup dapper response
            mockDapperUnitOfWork.Setup(x => x.KinMaiRepository.QueryAsync<ResArrayDataModel>(It.IsAny<string>()))
                                .Returns(() => Task.FromResult<IEnumerable<ResArrayDataModel>>(mockRestaurantInfoResponse));

            // act
            var actualOutput = await restaurantService.GetRestaurantDetail(mockRestaurant.Id);
            var expectedOutput = new RestaurantDetailModel()
            {
                RestaurantInfo = new Restaurant()
                {
                    Id = mockRestaurant.Id,
                    OwnerId = mockRestaurant.OwnerId,
                    Name = mockRestaurant.Name,
                    Description = mockRestaurant.Description,
                    Address = JsonConvert.SerializeObject(mockRestaurant.Address),
                    CreateAt = mockRestaurant.CreateAt,
                    RestaurantType = mockRestaurant.RestaurantType,
                    Latitude = mockRestaurant.Latitude,
                    Longitude = mockRestaurant.Longitude,
                    MinPriceRate = mockRestaurant.MinPriceRate,
                    MaxPriceRate = mockRestaurant.MaxPriceRate,
                    ImageLink = mockRestaurantInfoResponse[0].ImageLink,
                    DeliveryType = mockRestaurantInfoResponse[0].DeliveryType?.ToArray(),
                    PaymentMethod = mockRestaurantInfoResponse[0].PaymentMethod?.ToArray(),
                    Owner = mockUser
                },
                SocialContact = new List<SocialContactModel>()
                {
                    new SocialContactModel()
                    {
                        SocialType = mockSocialContact[0].SocialType,
                        ContactValue = mockSocialContact[0].ContactValue
                    }
                },
                Categories = new List<CategoryModel>()
                {
                    new CategoryModel()
                    {
                        CategoryType = mockCategory[0].Type,
                        CategoryName = mockCategory[0].Name
                    }
                },
                BusinessHours = mockBusinessHour,
            };

            // assert
            var actualOutputObj = JsonConvert.SerializeObject(actualOutput);
            var expetedOutputObj = JsonConvert.SerializeObject(expectedOutput);
            mockEntityUnitOfWork.VerifyAll();
            mockDapperUnitOfWork.VerifyAll();
            Assert.Equal(expetedOutputObj, actualOutputObj);
        }

        [Fact]
        public async Task GetRestaurantDetail_ThrowArgumentException_WhenRestaurantDoesNotExist()
        {
            // setup db response
            mockEntityUnitOfWork.Setup(x => x.RestaurantRepository.GetSingleAsync(It.IsAny<Expression<Func<Restaurant, bool>>>()))
                                .ReturnsAsync(() => null);

            // act
            Func<Task> actualOutput = () => restaurantService.GetRestaurantDetail(Guid.NewGuid());

            // assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(actualOutput);
            mockEntityUnitOfWork.VerifyAll();
            Assert.Equal("This restaurant does not exists.", exception.Message);
        }
    }
}
