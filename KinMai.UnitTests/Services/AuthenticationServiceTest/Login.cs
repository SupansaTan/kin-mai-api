using Amazon.CognitoIdentityProvider.Model;
using KinMai.Api.Models;
using KinMai.Authentication.Model;
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
using System.Net;

namespace KinMai.UnitTests.Services.AuthenticationServiceTest
{
    public class Login
    {
        private readonly InitConfiguration initConfiguration;
        private readonly Mock<IDapperUnitOfWork> mockDapperUnitOfWork;
        private readonly Mock<IS3UnitOfWork> mockS3UnitOfWork;
        private readonly Mock<IMailUnitOfWork> mockMailUnitOfWork;
        private readonly Mock<IAuthenticationUnitOfWork> mockAuthenticationUnitOfWork;
        private readonly Mock<IEntityUnitOfWork> mockEntityUnitOfWork;
        private readonly IAuthenticationService authenticationService;

        public Login()
        {
            initConfiguration = new InitConfiguration();
            mockDapperUnitOfWork = new Mock<IDapperUnitOfWork>();
            mockS3UnitOfWork = new Mock<IS3UnitOfWork>();
            mockMailUnitOfWork = new Mock<IMailUnitOfWork>();
            mockAuthenticationUnitOfWork = new Mock<IAuthenticationUnitOfWork>();
            mockEntityUnitOfWork = new Mock<IEntityUnitOfWork>();
            authenticationService =
                new AuthenticationService(
                    mockEntityUnitOfWork.Object,
                    mockAuthenticationUnitOfWork.Object,
                    mockDapperUnitOfWork.Object,
                    mockS3UnitOfWork.Object,
                    mockMailUnitOfWork.Object
               );
        }

        [Fact]
        public async Task Login_ReturnTokenModel_LoginByExistUser()
        {
            // mock data
            Guid userId;
            Guid.TryParse("9c16fe15-f21e-4071-94e8-c982b6c9c626", out userId);

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

            // arrange login model
            var mockRequest = new LoginModel()
            {
                Email = mockUser.Email,
                Password = "12345678",
            };

            // mock aws login response
            var awsLoginResponse = new InitiateAuthResponse()
            {
                HttpStatusCode = HttpStatusCode.OK,
                AuthenticationResult = new AuthenticationResultType()
                {
                    AccessToken = "accesstoken",
                    RefreshToken = "refreshtoken",
                    ExpiresIn = 3600,
                }
            };

            // setup db response
            mockEntityUnitOfWork.Setup(x => x.UserRepository.GetSingleAsync(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(() => mockUser);

            // setup aws cognito response
            mockAuthenticationUnitOfWork.Setup(x => x.AWSCognitoService.Login(It.IsAny<Guid>(), It.IsAny<string>()))
                                        .Returns(Task.FromResult(awsLoginResponse));

            // act
            var actualOutput = await authenticationService.Login(mockUser.Email, mockRequest.Password);
            var expectOutput = new TokenResponseModel()
            {
                Token = awsLoginResponse.AuthenticationResult.AccessToken,
                ExpiredToken = actualOutput.ExpiredToken,
                RefreshToken = awsLoginResponse.AuthenticationResult.RefreshToken
            };

            // assert
            mockEntityUnitOfWork.VerifyAll();
            mockAuthenticationUnitOfWork.VerifyAll();
            Assert.Equal(expectOutput.Token, actualOutput.Token);
            Assert.Equal(expectOutput.RefreshToken, actualOutput.RefreshToken);
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
            mockEntityUnitOfWork.Setup(x => x.UserRepository.GetSingleAsync(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(() => mockUser);

            // setup aws cognito response
            mockAuthenticationUnitOfWork.Setup(x => x.AWSCognitoService.Login(It.IsAny<Guid>(), It.IsAny<string>()))
                                        .Returns(() => null);

            // act
            Func<Task> act = () => authenticationService.Login(mockUser.Email, password);

            // assert
            var exception = await Assert.ThrowsAsync<Exception>(act);
            mockEntityUnitOfWork.VerifyAll();
            mockAuthenticationUnitOfWork.VerifyAll();
            Assert.Equal("Invalid Email or password.", exception.Message);
        }

        [Fact]
        public async Task Login_ThrowArgumentException_LoginByNewUser()
        {
            // mock db repository & service
            mockEntityUnitOfWork.Setup(x => x.UserRepository.GetSingleAsync(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(() => null);

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
            mockEntityUnitOfWork.Setup(x => x.UserRepository.GetSingleAsync(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(() => mockUser);

            // act
            Func<Task> act = () => authenticationService.Login(mockUser.Email, password);

            // assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(act);
            mockEntityUnitOfWork.VerifyAll();
            Assert.Equal("This email is registered by Google provider, Please login by Google instead", exception.Message);
        }
    }   
}
