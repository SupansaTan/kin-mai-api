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
using System.Text;

namespace KinMai.UnitTests.Services.AuthenticationServiceTest
{
    public class ResetPassword
    {
        private readonly InitConfiguration initConfiguration;
        private Mock<IDapperUnitOfWork> mockDapperUnitOfWork;
        private Mock<IS3UnitOfWork> mockS3UnitOfWork;
        private Mock<IMailUnitOfWork> mockMailUnitOfWork;
        private Mock<IAuthenticationUnitOfWork> mockAuthenticationUnitOfWork;
        private Mock<IEntityUnitOfWork> mockEntityUnitOfWork;
        private IAuthenticationService authenticationService;

        public ResetPassword()
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
        public async Task ResetPassword_ReturnTrue_WhenRequestValidModel()
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

            var mockRequest = new ResetPasswordModel()
            {
                ResetToken = EncodeBase64String(mockUser.Id.ToString()),
                Password = "123456789",
                ConfirmPassword = "123456789"
            };

            // setup database response
            mockEntityUnitOfWork.Setup(x => x.UserRepository.GetSingleAsync(It.IsAny<Expression<Func<User, bool>>>()))
                                .ReturnsAsync(() => mockUser);

            // setup aws cognito response
            mockAuthenticationUnitOfWork.Setup(x => x.AWSCognitoService.ResetPassword(It.IsAny<Guid>(), It.IsAny<string>()))
                                        .Returns(Task.FromResult(true));

            // act
            var actualOutput = await authenticationService.ResetPassword(mockRequest);

            // assert
            mockEntityUnitOfWork.VerifyAll();
            mockAuthenticationUnitOfWork.VerifyAll();
            Assert.True(actualOutput);
        }

        [Fact]
        public async Task ResetPassword_ReturnFalse_WhenCanNotChangePasswordInAwsCognito()
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

            var mockRequest = new ResetPasswordModel()
            {
                ResetToken = EncodeBase64String(mockUser.Id.ToString()),
                Password = "123456789",
                ConfirmPassword = "123456789"
            };

            // setup database response
            mockEntityUnitOfWork.Setup(x => x.UserRepository.GetSingleAsync(It.IsAny<Expression<Func<User, bool>>>()))
                                .ReturnsAsync(() => mockUser);

            // setup aws cognito response
            mockAuthenticationUnitOfWork.Setup(x => x.AWSCognitoService.ResetPassword(It.IsAny<Guid>(), It.IsAny<string>()))
                                        .Returns(Task.FromResult(false));

            // act
            var actualOutput = await authenticationService.ResetPassword(mockRequest);

            // assert
            mockEntityUnitOfWork.VerifyAll();
            mockAuthenticationUnitOfWork.VerifyAll();
            Assert.False(actualOutput);
        }

        [Fact]
        public async Task ResetPassword_ThrowArgumentException_WhenUserDoesNotExist()
        {
            // setup database response
            mockEntityUnitOfWork.Setup(x => x.UserRepository.GetSingleAsync(It.IsAny<Expression<Func<User, bool>>>()))
                                .ReturnsAsync(() => null);

            // arrange
            var mockRequest = new ResetPasswordModel()
            {
                ResetToken = EncodeBase64String(Guid.NewGuid().ToString()),
                Password = "123456789",
                ConfirmPassword = "123456789"
            };

            // act
            Func<Task> act = () => authenticationService.ResetPassword(mockRequest);

            // assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(act);
            mockEntityUnitOfWork.VerifyAll();
            Assert.Equal("User does not exists.", exception.Message);
        }

        [Fact]
        public async Task ResetPassword_ThrowArgumentException_WhenPasswordNotMatching()
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

            var mockRequest = new ResetPasswordModel()
            {
                ResetToken = EncodeBase64String(Guid.NewGuid().ToString()),
                Password = "123456789",
                ConfirmPassword = "12345678911"
            };

            // setup database response
            mockEntityUnitOfWork.Setup(x => x.UserRepository.GetSingleAsync(It.IsAny<Expression<Func<User, bool>>>()))
                                .ReturnsAsync(() => mockUser);

            // act
            Func<Task> act = () => authenticationService.ResetPassword(mockRequest);

            // assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(act);
            mockEntityUnitOfWork.VerifyAll();
            Assert.Equal("Password and Confirm password are not matching", exception.Message);
        }

        private string EncodeBase64String(string text)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(text));
        }
    }
}
