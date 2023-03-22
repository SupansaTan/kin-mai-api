using KinMai.Authentication.UnitOfWork;
using KinMai.Dapper.Interface;
using KinMai.EntityFramework.Models;
using KinMai.EntityFramework.UnitOfWork.Interface;
using KinMai.Logic.Interface;
using KinMai.Logic.Models;
using KinMai.Logic.Services;
using KinMai.Mail.UnitOfWork;
using KinMai.S3.UnitOfWork.Interface;
using KinMai.UnitTests.Shared;
using Moq;
using System.Linq.Expressions;

namespace KinMai.UnitTests.Services.AuthenticationServiceTest
{
    public class GetUserInfo
    {
        private readonly InitConfiguration initConfiguration;
        private Mock<IDapperUnitOfWork> mockDapperUnitOfWork;
        private Mock<IS3UnitOfWork> mockS3UnitOfWork;
        private Mock<IMailUnitOfWork> mockMailUnitOfWork;
        private Mock<IAuthenticationUnitOfWork> mockAuthenticationUnitOfWork;
        private Mock<IEntityUnitOfWork> mockEntityUnitOfWork;
        private IAuthenticationService authenticationService;

        public GetUserInfo()
        {
            initConfiguration = new InitConfiguration();
            mockDapperUnitOfWork = new Mock<IDapperUnitOfWork>();
            mockS3UnitOfWork = new Mock<IS3UnitOfWork>();
            mockMailUnitOfWork = new Mock<IMailUnitOfWork>();
            mockAuthenticationUnitOfWork = new Mock<IAuthenticationUnitOfWork>();
            mockEntityUnitOfWork = new Mock<IEntityUnitOfWork>();
            authenticationService = new AuthenticationService(
                mockEntityUnitOfWork.Object,
                mockAuthenticationUnitOfWork.Object,
                mockDapperUnitOfWork.Object,
                mockS3UnitOfWork.Object,
                mockMailUnitOfWork.Object
            );
        }

        [Fact]
        public async Task RestaurantRegister_ReturnUserInfoModel_WhenEmailIsExist()
        {
            // arrange
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

            // setup database response
            mockEntityUnitOfWork.Setup(x => x.UserRepository.GetSingleAsync(It.IsAny<Expression<Func<User, bool>>>()))
                                .ReturnsAsync(() => mockUser);

            // act
            var actualOutput = await authenticationService.GetUserInfo("nampunch1@gmail.com");
            var expectedOutput = new UserInfoModel()
            {
                UserId = mockUser.Id,
                UserName = mockUser.Username,
                RestaurantName = "",
                UserType = (Common.Enum.UserType)mockUser.UserType
            };

            // assert
            mockEntityUnitOfWork.VerifyAll();
            Assert.Equal(expectedOutput.UserId, actualOutput.UserId);
            Assert.Equal(expectedOutput.UserName, actualOutput.UserName);
            Assert.Equal(expectedOutput.RestaurantName, actualOutput.RestaurantName);
            Assert.Equal(expectedOutput.UserType, actualOutput.UserType);
        }

        [Fact]
        public async Task RestaurantRegister_ThrowArgumentException_WhenEmailDoesNotExist()
        {
            // setup database response
            mockEntityUnitOfWork.Setup(x => x.UserRepository.GetSingleAsync(It.IsAny<Expression<Func<User, bool>>>()))
                                .ReturnsAsync(() => null);

            // act
            Func<Task> act = () => authenticationService.GetUserInfo("nampunch1@gmail.com");

            // assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(act);
            mockEntityUnitOfWork.VerifyAll();
            Assert.Equal("Email does not exist.", exception.Message);
        }
    }
}
