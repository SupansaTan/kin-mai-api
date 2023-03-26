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
    public class CheckIsLoginWithGoogleFirstTimes
    {
        private readonly InitConfiguration initConfiguration;
        private Mock<IDapperUnitOfWork> mockDapperUnitOfWork;
        private Mock<IS3UnitOfWork> mockS3UnitOfWork;
        private Mock<IMailUnitOfWork> mockMailUnitOfWork;
        private Mock<IAuthenticationUnitOfWork> mockAuthenticationUnitOfWork;
        private Mock<IEntityUnitOfWork> mockEntityUnitOfWork;
        private IAuthenticationService authenticationService;

        public CheckIsLoginWithGoogleFirstTimes()
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
        public async Task CheckIsLoginWithGoogleFirstTimes_ReturnFalse_WhenUserIsExist()
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
                IsLoginWithGoogle = true
            };

            // setup database response
            mockEntityUnitOfWork.Setup(x => x.UserRepository.GetSingleAsync(It.IsAny<Expression<Func<User, bool>>>()))
                                .ReturnsAsync(() => mockUser);

            // act
            var actualOutput = await authenticationService.CheckIsLoginWithGoogleFirstTimes(mockUser.Email);

            // assert
            mockEntityUnitOfWork.VerifyAll();
            Assert.False(actualOutput);
        }

        [Fact]
        public async Task CheckIsLoginWithGoogleFirstTimes_ReturnTrue_WhenUserDoesNotExist()
        {
            // setup database response
            mockEntityUnitOfWork.Setup(x => x.UserRepository.GetSingleAsync(It.IsAny<Expression<Func<User, bool>>>()))
                                .ReturnsAsync(() => null);

            // act
            var actualOutput = await authenticationService.CheckIsLoginWithGoogleFirstTimes("nampunch1@gmail.com");

            // assert
            mockEntityUnitOfWork.VerifyAll();
            Assert.True(actualOutput);
        }
    }
}
