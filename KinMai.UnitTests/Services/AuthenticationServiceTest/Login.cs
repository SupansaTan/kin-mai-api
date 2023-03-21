using KinMai.Authentication.UnitOfWork;
using KinMai.Dapper.Interface;
using KinMai.EntityFramework.Models;
using KinMai.EntityFramework.UnitOfWork.Interface;
using KinMai.Logic.Interface;
using KinMai.Logic.Services;
using KinMai.Mail.UnitOfWork;
using KinMai.S3.UnitOfWork.Interface;
using KinMai.UnitTests.Shared;
using Moq;
using System.Linq.Expressions;

namespace KinMai.UnitTests.Services.AuthenticationServiceTest
{
    public class Login
    {
        private readonly InitConfiguration initConfiguration;
        private readonly Mock<IDapperUnitOfWork> mockDapperUnitOfWork;
        private readonly Mock<IS3UnitOfWork> mockS3UnitOfWork;
        private readonly Mock<IMailUnitOfWork> mockMailUnitOfWork;
        private IAuthenticationUnitOfWork mockAuthenticationUnitOfWork;

        public Login()
        {
            initConfiguration = new InitConfiguration();
            mockDapperUnitOfWork = new Mock<IDapperUnitOfWork>();
            mockS3UnitOfWork = new Mock<IS3UnitOfWork>();
            mockMailUnitOfWork = new Mock<IMailUnitOfWork>();
            mockAuthenticationUnitOfWork = new AuthenticationUnitOfWork();
        }

        [Fact]
        public async Task Login_ReturnTokenModel_LoginByExistUser()
        {
            // mock data
            Guid userId;
            Guid.TryParse("9c16fe15-f21e-4071-94e8-c982b6c9c626", out userId);
            var password = "12345678";

            // add user
            var mockUser = new User()
            {
                Id = userId,
                Email = "nampunch1@gmail.com",
                FirstName = "Supansa",
                LastName = "Tantulset",
                Username = "littlepunchhz",
                CreateAt = DateTime.UtcNow,
                UserType = 1,
                IsLoginWithGoogle = false
            };

            // mock db repository & service
            var mockEntityUnitOfWork = new Mock<IEntityUnitOfWork>();
            mockEntityUnitOfWork.Setup(x => x.UserRepository.GetSingleAsync(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(() => mockUser);
            
            IAuthenticationService authenticationService =
                new AuthenticationService(
                    mockEntityUnitOfWork.Object,
                    mockAuthenticationUnitOfWork,
                    mockDapperUnitOfWork.Object,
                    mockS3UnitOfWork.Object,
                    mockMailUnitOfWork.Object
               );

            // act
            var actualOutput = await authenticationService.Login(mockUser.Email, password);

            // assert
            mockEntityUnitOfWork.VerifyAll();
            Assert.NotNull(actualOutput);
        }

        [Fact]
        public async Task Login_ThrowArgumentException_LoginByIncorrectPassword()
        {
            // mock data
            Guid userId;
            Guid.TryParse("9c16fe15-f21e-4071-94e8-c982b6c9c626", out userId);
            var password = "11111111";

            // add user
            var mockUser = new User()
            {
                Id = userId,
                Email = "nampunch1@gmail.com",
                FirstName = "Supansa",
                LastName = "Tantulset",
                Username = "littlepunchhz",
                CreateAt = DateTime.UtcNow,
                UserType = 1,
                IsLoginWithGoogle = false
            };

            // mock db repository & service
            var mockEntityUnitOfWork = new Mock<IEntityUnitOfWork>();
            mockEntityUnitOfWork.Setup(x => x.UserRepository.GetSingleAsync(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(() => mockUser);

            IAuthenticationService authenticationService =
                new AuthenticationService(
                    mockEntityUnitOfWork.Object,
                    mockAuthenticationUnitOfWork,
                    mockDapperUnitOfWork.Object,
                    mockS3UnitOfWork.Object,
                    mockMailUnitOfWork.Object
               );

            // act
            Func<Task> act = () => authenticationService.Login(mockUser.Email, password);

            // assert
            var exception = await Assert.ThrowsAsync<Exception>(act);
            mockEntityUnitOfWork.VerifyAll();
            Assert.Equal("Invalid Email or password.", exception.Message);
        }

        [Fact]
        public async Task Login_ThrowArgumentException_LoginByNewUser()
        {
            // mock db repository & service
            var mockEntityUnitOfWork = new Mock<IEntityUnitOfWork>();
            mockEntityUnitOfWork.Setup(x => x.UserRepository.GetSingleAsync(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(() => null);

            IAuthenticationService authenticationService =
                new AuthenticationService(
                    mockEntityUnitOfWork.Object,
                    mockAuthenticationUnitOfWork,
                    mockDapperUnitOfWork.Object,
                    mockS3UnitOfWork.Object,
                    mockMailUnitOfWork.Object
               );

            // arrange
            var email = "nampunch1@gmail.com";
            var password = "12345678";

            // act
            Func<Task> act = () => authenticationService.Login(email, password);

            // assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(act);
            mockEntityUnitOfWork.VerifyAll();
            Assert.Equal("Email does not exist.", exception.Message);
        }

        [Fact]
        public async Task Login_ThrowArgumentException_LoginByUserRegisteredByGoogleAccount()
        {
            // arrange
            var password = "11111111";
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

            // mock db repository & service
            var mockEntityUnitOfWork = new Mock<IEntityUnitOfWork>();
            mockEntityUnitOfWork.Setup(x => x.UserRepository.GetSingleAsync(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(() => mockUser);

            IAuthenticationService authenticationService =
                new AuthenticationService(
                    mockEntityUnitOfWork.Object,
                    mockAuthenticationUnitOfWork,
                    mockDapperUnitOfWork.Object,
                    mockS3UnitOfWork.Object,
                    mockMailUnitOfWork.Object
               );

            // act
            Func<Task> act = () => authenticationService.Login(mockUser.Email, password);

            // assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(act);
            mockEntityUnitOfWork.VerifyAll();
            Assert.Equal("This email is registered by Google provider, Please login by Google instead", exception.Message);
        }
    }   
}
