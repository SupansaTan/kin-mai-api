using Amazon.CognitoIdentityProvider.Model;
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
using System.Net;

namespace KinMai.UnitTests.Controllers.AuthenticationControllerTest
{
    public class ReviewerRegister
    {
        private readonly InitConfiguration initConfiguration;
        private readonly Mock<IDapperUnitOfWork> mockDapperUnitOfWork;
        private readonly Mock<IS3UnitOfWork> mockS3UnitOfWork;
        private readonly Mock<IMailUnitOfWork> mockMailUnitOfWork;
        private readonly Mock<IAuthenticationUnitOfWork> mockAuthenticationUnitOfWork;
        private readonly Mock<IEntityUnitOfWork> mockEntityUnitOfWork;
        private readonly ILogicUnitOfWork logicUnitOfWork;
        private readonly AuthenticationController authenticationController;

        public ReviewerRegister()
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
        public async Task ReviewerRegister_ReturnStatus200_WhenRegisterWithValidModel()
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

            // setup db repository response
            mockEntityUnitOfWork.Setup(x => x.UserRepository.GetSingleAsync(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(() => null);
            mockEntityUnitOfWork.Setup(x => x.UserRepository.Add(It.IsAny<User>()));

            // setup aws cognito response
            mockAuthenticationUnitOfWork.Setup(x => x.AWSCognitoService.SignUp(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
                                        .Returns(Task.FromResult(mockSignUpResponse));
            mockAuthenticationUnitOfWork.Setup(x => x.AWSCognitoService.ConfirmSignUp(It.IsAny<Guid>()))
                                        .Returns(Task.FromResult(true));

            // act
            var actualOutput = await authenticationController.ReviewerRegister(mockRequest);
            var expectOutput = new ResponseModel<bool>()
            {
                Data = true,
                Message = "success",
                Status = 200
            };

            // assert
            Assert.Equal(actualOutput.Data, expectOutput.Data);
            Assert.Equal(actualOutput.Message, expectOutput.Message);
            Assert.Equal(actualOutput.Status, expectOutput.Status);
        }

        [Fact]
        public async Task ReviewerRegister_ReturnStatus200_WhenRegisterWithGoogleAccount()
        {
            // setup db repository response
            mockEntityUnitOfWork.Setup(x => x.UserRepository.GetSingleAsync(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(() => null);
            mockEntityUnitOfWork.Setup(x => x.UserRepository.Add(It.IsAny<User>()));

            // arrange
            var mockRequest = new ReviewerRegisterModel()
            {
                Email = "nampunch1@gmail.com",
                FirstName = "Supansa",
                LastName = "Tantulset",
                Username = "littlepunchhz"
            };

            // act
            var actualOutput = await authenticationController.ReviewerRegister(mockRequest);
            var expectOutput = new ResponseModel<bool>()
            {
                Data = true,
                Message = "success",
                Status = 200
            };

            // assert
            Assert.Equal(actualOutput.Data, expectOutput.Data);
            Assert.Equal(actualOutput.Message, expectOutput.Message);
            Assert.Equal(actualOutput.Status, expectOutput.Status);
        }

        [Fact]
        public async Task ReviewerRegister_ReturnStatus400_WhenPasswordDoNotMatch()
        {
            // setup db repository response
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
            var actualOutput = await authenticationController.ReviewerRegister(mockRequest);
            var expectOutput = new ResponseModel<bool>()
            {
                Data = false,
                Message = "Password and Confirm password are not matching",
                Status = 400
            };

            // assert
            Assert.Equal(actualOutput.Data, expectOutput.Data);
            Assert.Equal(actualOutput.Message, expectOutput.Message);
            Assert.Equal(actualOutput.Status, expectOutput.Status);
        }

        [Fact]
        public async Task ReviewerRegister_ReturnStatus400_WhenRegisterWithExistEmail()
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

            // setup db repository response
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
            var actualOutput = await authenticationController.ReviewerRegister(mockRequest);
            var expectOutput = new ResponseModel<bool>()
            {
                Data = false,
                Message = "Email already exists.",
                Status = 400
            };

            // assert
            Assert.Equal(actualOutput.Data, expectOutput.Data);
            Assert.Equal(actualOutput.Message, expectOutput.Message);
            Assert.Equal(actualOutput.Status, expectOutput.Status);
        }
    }
}
