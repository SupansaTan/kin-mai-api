using Amazon.CognitoIdentityProvider.Model;
using KinMai.Api.Controllers;
using KinMai.Api.Models;
using KinMai.Authentication.Model;
using KinMai.Authentication.UnitOfWork;
using KinMai.Dapper.Interface;
using KinMai.EntityFramework.Models;
using KinMai.EntityFramework.UnitOfWork.Interface;
using KinMai.Logic.UnitOfWork.Implement;
using KinMai.Logic.UnitOfWork.Interface;
using KinMai.Mail.UnitOfWork;
using KinMai.S3.UnitOfWork.Interface;
using KinMai.UnitTests.Shared;
using Moq;
using System.Linq.Expressions;
using System.Net;

namespace KinMai.UnitTests.Controllers.AuthenticationControllerTest
{
    public class Login
    {
        private readonly InitConfiguration initConfiguration;
        private readonly Mock<IDapperUnitOfWork> mockDapperUnitOfWork;
        private readonly Mock<IS3UnitOfWork> mockS3UnitOfWork;
        private readonly Mock<IMailUnitOfWork> mockMailUnitOfWork;
        private readonly Mock<IAuthenticationUnitOfWork> mockAuthenticationUnitOfWork;
        private readonly Mock<IEntityUnitOfWork> mockEntityUnitOfWork;
        private readonly ILogicUnitOfWork logicUnitOfWork;
        private readonly AuthenticationController authenticationController;

        public Login()
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
            authenticationController = new AuthenticationController(logicUnitOfWork);
        }

        [Fact]
        public async Task Login_ReturnStatus200WithTokenModel_WhenLoginByExistUser()
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
            var actualOutput = await authenticationController.Login(mockRequest);
            var expectOutput = new ResponseModel<TokenResponseModel>()
            {
                Data = new TokenResponseModel()
                {
                    Token = awsLoginResponse.AuthenticationResult.AccessToken,
                    ExpiredToken = actualOutput.Data.ExpiredToken,
                    RefreshToken = awsLoginResponse.AuthenticationResult.RefreshToken
                },
                Message = "success",
                Status = 200
            };

            // assert
            Assert.Equal(expectOutput.Data.Token, actualOutput.Data.Token);
            Assert.Equal(expectOutput.Data.RefreshToken, actualOutput.Data.RefreshToken);
            Assert.Equal(expectOutput.Message, actualOutput.Message);
            Assert.Equal(expectOutput.Status, actualOutput.Status);
        }

        [Fact]
        public async Task Login_ReturnStatus500_WhenLoginWithIncorrectPassword()
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
                Password = "11111111",
            };

            // mock db repository & controller
            mockEntityUnitOfWork.Setup(x => x.UserRepository.GetSingleAsync(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(() => mockUser);

            // setup aws cognito response
            mockAuthenticationUnitOfWork.Setup(x => x.AWSCognitoService.Login(It.IsAny<Guid>(), It.IsAny<string>()))
                                        .Returns(() => null);

            // act
            var actualOutput = await authenticationController.Login(mockRequest);
            var expectOutput = new ResponseModel<TokenResponseModel>()
            {
                Data = null,
                Message = "Invalid Email or password.",
                Status = 500
            };

            // assert
            Assert.Equal(actualOutput.Data, expectOutput.Data);
            Assert.Equal(actualOutput.Message, expectOutput.Message);
            Assert.Equal(actualOutput.Status, expectOutput.Status);
        }

        [Fact]
        public async Task Login_ReturnStatus400_WhenLoginByNewUser()
        {
            // arrange
            var mockRequest = new LoginModel()
            {
                Email = "nampunch1@gmail.com",
                Password = "11111111",
            };

            // mock db repository & controller
            mockEntityUnitOfWork.Setup(x => x.UserRepository.GetSingleAsync(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(() => null);

            // act
            var actualOutput = await authenticationController.Login(mockRequest);
            var expectOutput = new ResponseModel<TokenResponseModel>()
            {
                Data = null,
                Message = "Email does not exist.",
                Status = 400
            };

            // assert
            Assert.Equal(actualOutput.Data, expectOutput.Data);
            Assert.Equal(actualOutput.Message, expectOutput.Message);
            Assert.Equal(actualOutput.Status, expectOutput.Status);
        }

        [Fact]
        public async Task Login_ReturnStatus400_WhenLoginByUserRegisteredWithGoogleAccount()
        {
            // mock user
            Guid userId;
            Guid.TryParse("9c16fe15-f21e-4071-94e8-c982b6c9c626", out userId);
            var mockUser = new User()
            {
                Id = userId,
                Email = "nampunch1@gmail.com",
                FirstName = "Supansa",
                LastName = "Tantulset",
                Username = "littlepunchhz",
                CreateAt = DateTime.UtcNow,
                UserType = 1,
                IsLoginWithGoogle = true
            };

            // mock db repository & controller
            mockEntityUnitOfWork.Setup(x => x.UserRepository.GetSingleAsync(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(() => mockUser);

            // arrange login model
            var mockRequest = new LoginModel()
            {
                Email = mockUser.Email,
                Password = "11111111",
            };

            // act
            var actualOutput = await authenticationController.Login(mockRequest);
            var expectOutput = new ResponseModel<TokenResponseModel>()
            {
                Data = null,
                Message = "This email is registered by Google provider, Please login by Google instead",
                Status = 400
            };

            // assert
            Assert.Equal(actualOutput.Data, expectOutput.Data);
            Assert.Equal(actualOutput.Message, expectOutput.Message);
            Assert.Equal(actualOutput.Status, expectOutput.Status);
        }
    }
}
