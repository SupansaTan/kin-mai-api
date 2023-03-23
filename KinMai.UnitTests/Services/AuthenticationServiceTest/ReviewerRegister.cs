using Amazon.CognitoIdentityProvider.Model;
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
using System.Net;

namespace KinMai.UnitTests.Services.AuthenticationServiceTest
{
    public class ReviewerRegister
    {
        private readonly InitConfiguration initConfiguration;
        private readonly Mock<IDapperUnitOfWork> mockDapperUnitOfWork;
        private readonly Mock<IS3UnitOfWork> mockS3UnitOfWork;
        private readonly Mock<IMailUnitOfWork> mockMailUnitOfWork;
        private readonly Mock<IAuthenticationUnitOfWork> mockAuthenticationUnitOfWork;
        private readonly Mock<IEntityUnitOfWork> mockEntityUnitOfWork;
        private readonly IAuthenticationService authenticationService;

        public ReviewerRegister()
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
        public async Task ReviewerRegister_ReturnTrue_WhenRegisterWithValidModel()
        {
            // arrange
            var mockRequest = new ReviewerRegisterModel()
            {
                Email = "nampunch1@gmail.com",
                FirstName = "Supansa",
                LastName = "Tantulset",
                Username = "littlepunchhz",
                Password = "12345678",
                ConfirmPassword = "12345678"
            };

            var mockSignUpResponse = new SignUpResponse()
            {
                HttpStatusCode = HttpStatusCode.OK
            };

            // mock db repository & service
            mockEntityUnitOfWork.Setup(x => x.UserRepository.GetSingleAsync(It.IsAny<Expression<Func<User,bool>>>())).ReturnsAsync(() => null);
            mockEntityUnitOfWork.Setup(x => x.UserRepository.Add(It.IsAny<User>()));

            // setup aws cognito response
            mockAuthenticationUnitOfWork.Setup(x => x.AWSCognitoService.SignUp(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
                                        .Returns(Task.FromResult(mockSignUpResponse));
            mockAuthenticationUnitOfWork.Setup(x => x.AWSCognitoService.ConfirmSignUp(It.IsAny<Guid>()))
                                        .Returns(Task.FromResult(true));

            // act
            var actualOutput = await authenticationService.ReviewerRegister(mockRequest);

            // assert
            mockEntityUnitOfWork.VerifyAll();
            mockAuthenticationUnitOfWork.VerifyAll();
            Assert.True(actualOutput);
        }

        [Fact]
        public async Task ReviewerRegister_ReturnTrue_WhenRegisterWithGoogleAccount()
        {
            // arrange
            var mockRequest = new ReviewerRegisterModel()
            {
                Email = "nampunch1@gmail.com",
                FirstName = "Supansa",
                LastName = "Tantulset",
                Username = "littlepunchhz"
            };

            // mock db repository & service
            mockEntityUnitOfWork.Setup(x => x.UserRepository.GetSingleAsync(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(() => null);
            mockEntityUnitOfWork.Setup(x => x.UserRepository.Add(It.IsAny<User>()));

            // act
            var actualOutput = await authenticationService.ReviewerRegister(mockRequest);

            // assert
            mockEntityUnitOfWork.VerifyAll();
            Assert.True(actualOutput);
        }

        [Fact]
        public async Task ReviewerRegister_ThrowArgumentException_WhenPasswordDoNotMatch()
        {
            // mock db repository & service
            mockEntityUnitOfWork.Setup(x => x.UserRepository.GetSingleAsync(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(() => null);

            // arrange
            var mockRequest = new ReviewerRegisterModel()
            {
                Email = "nampunch1@gmail.com",
                FirstName = "Supansa",
                LastName = "Tantulset",
                Username = "littlepunchhz",
                Password = "12345678",
                ConfirmPassword = "123456789"
            };

            // act
            Func<Task> act = () => authenticationService.ReviewerRegister(mockRequest);

            // assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(act);
            mockEntityUnitOfWork.VerifyAll();
            Assert.Equal("Password and Confirm password are not matching", exception.Message);
        }

        [Fact]
        public async Task ReviewerRegister_ThrowArgumentException_WhenRegisterWithExistEmail()
        {
            var mockExistUser = new User()
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

            // mock db repository & service
            mockEntityUnitOfWork.Setup(x => x.UserRepository.GetSingleAsync(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(() => mockExistUser);

            // arrange
            var mockRequest = new ReviewerRegisterModel()
            {
                Email = "nampunch1@gmail.com",
                FirstName = "Supansa",
                LastName = "Tantulset",
                Username = "littlepunchhz",
                Password = "12345678",
                ConfirmPassword = "12345678"
            };

            // act
            Func<Task> act = () => authenticationService.ReviewerRegister(mockRequest);

            // assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(act);
            mockEntityUnitOfWork.VerifyAll();
            Assert.Equal("Email already exists.", exception.Message);
        }
    }
}
