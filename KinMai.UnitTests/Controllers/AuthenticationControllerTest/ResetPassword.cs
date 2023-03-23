using KinMai.Api.Controllers;
using KinMai.Api.Models;
using KinMai.Authentication.UnitOfWork;
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
using System.Linq.Expressions;
using System.Text;

namespace KinMai.UnitTests.Controllers.AuthenticationControllerTest
{
    public class ResetPassword
    {
        private readonly InitConfiguration initConfiguration;
        private readonly Mock<IDapperUnitOfWork> mockDapperUnitOfWork;
        private readonly Mock<IS3UnitOfWork> mockS3UnitOfWork;
        private readonly Mock<IMailUnitOfWork> mockMailUnitOfWork;
        private readonly Mock<IAuthenticationUnitOfWork> mockAuthenticationUnitOfWork;
        private readonly Mock<IEntityUnitOfWork> mockEntityUnitOfWork;
        private readonly ILogicUnitOfWork logicUnitOfWork;
        private readonly AuthenticationController authenticationController;

        public ResetPassword()
        {
            initConfiguration = new InitConfiguration();
            mockDapperUnitOfWork = new Mock<IDapperUnitOfWork>();
            mockS3UnitOfWork = new Mock<IS3UnitOfWork>();
            mockMailUnitOfWork = new Mock<IMailUnitOfWork>();
            mockAuthenticationUnitOfWork = new Mock<IAuthenticationUnitOfWork>();
            mockEntityUnitOfWork = new Mock<IEntityUnitOfWork>();
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
        public async Task ResetPassword_ReturnStatus200_WhenRequestValidModel()
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
            var actualOutput = await authenticationController.ResetPassword(mockRequest);
            var expectOutput = new ResponseModel<bool>()
            {
                Data = true,
                Message = "success",
                Status = 200
            };

            // assert
            Assert.Equal(expectOutput.Data, actualOutput.Data);
            Assert.Equal(expectOutput.Message, actualOutput.Message);
            Assert.Equal(expectOutput.Status, actualOutput.Status);
        }

        [Fact]
        public async Task ResetPassword_ReturnStatus200_WhenCanNotChangePasswordInAwsCognito()
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
            var actualOutput = await authenticationController.ResetPassword(mockRequest);
            var expectOutput = new ResponseModel<bool>()
            {
                Data = false,
                Message = "success",
                Status = 200
            };

            // assert
            Assert.Equal(expectOutput.Data, actualOutput.Data);
            Assert.Equal(expectOutput.Message, actualOutput.Message);
            Assert.Equal(expectOutput.Status, actualOutput.Status);
        }

        [Fact]
        public async Task ResetPassword_ReturnStatus400_WhenUserDoesNotExist()
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
            var actualOutput = await authenticationController.ResetPassword(mockRequest);
            var expectOutput = new ResponseModel<bool>()
            {
                Data = false,
                Message = "User does not exists.",
                Status = 400
            };

            // assert
            Assert.Equal(expectOutput.Data, actualOutput.Data);
            Assert.Equal(expectOutput.Message, actualOutput.Message);
            Assert.Equal(expectOutput.Status, actualOutput.Status);
        }

        [Fact]
        public async Task ResetPassword_ReturnStatus400_WhenPasswordNotMatching()
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
            var actualOutput = await authenticationController.ResetPassword(mockRequest);
            var expectOutput = new ResponseModel<bool>()
            {
                Data = false,
                Message = "Password and Confirm password are not matching",
                Status = 400
            };

            // assert
            Assert.Equal(expectOutput.Data, actualOutput.Data);
            Assert.Equal(expectOutput.Message, actualOutput.Message);
            Assert.Equal(expectOutput.Status, actualOutput.Status);
        }

        private string EncodeBase64String(string text)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(text));
        }
    }
}
