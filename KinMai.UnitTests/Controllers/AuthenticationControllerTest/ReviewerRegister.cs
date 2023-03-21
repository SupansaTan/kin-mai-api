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

namespace KinMai.UnitTests.Controllers.AuthenticationControllerTest
{
    public class ReviewerRegister
    {
        private readonly InitConfiguration initConfiguration;
        private readonly Mock<IDapperUnitOfWork> mockDapperUnitOfWork;
        private readonly Mock<IS3UnitOfWork> mockS3UnitOfWork;
        private readonly Mock<IMailUnitOfWork> mockMailUnitOfWork;
        private readonly IAuthenticationUnitOfWork mockAuthenticationUnitOfWork;

        public ReviewerRegister()
        {
            initConfiguration = new InitConfiguration();
            mockDapperUnitOfWork = new Mock<IDapperUnitOfWork>();
            mockS3UnitOfWork = new Mock<IS3UnitOfWork>();
            mockMailUnitOfWork = new Mock<IMailUnitOfWork>();
            mockAuthenticationUnitOfWork = new AuthenticationUnitOfWork();
        }

        [Fact]
        public async Task ReviewerRegister_ReturnStatus200_WhenRegisterWithValidModel()
        {
            // mock db repository
            var mockEntityUnitOfWork = new Mock<IEntityUnitOfWork>();
            mockEntityUnitOfWork.Setup(x => x.UserRepository.GetSingleAsync(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(() => null);
            mockEntityUnitOfWork.Setup(x => x.UserRepository.Add(It.IsAny<User>()));

            // init controller
            ILogicUnitOfWork logicUnitOfWork = new LogicUnitOfWork(
                mockEntityUnitOfWork.Object,
                mockDapperUnitOfWork.Object,
                mockAuthenticationUnitOfWork,
                mockS3UnitOfWork.Object,
                mockMailUnitOfWork.Object
            );
            var authenticationController = new AuthenticationController(logicUnitOfWork);

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
            // mock db repository & service
            var mockEntityUnitOfWork = new Mock<IEntityUnitOfWork>();
            mockEntityUnitOfWork.Setup(x => x.UserRepository.GetSingleAsync(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(() => null);
            mockEntityUnitOfWork.Setup(x => x.UserRepository.Add(It.IsAny<User>()));

            // init controller
            ILogicUnitOfWork logicUnitOfWork = new LogicUnitOfWork(
                mockEntityUnitOfWork.Object,
                mockDapperUnitOfWork.Object,
                mockAuthenticationUnitOfWork,
                mockS3UnitOfWork.Object,
                mockMailUnitOfWork.Object
            );
            var authenticationController = new AuthenticationController(logicUnitOfWork);

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
            // mock db repository & service
            var mockEntityUnitOfWork = new Mock<IEntityUnitOfWork>();
            mockEntityUnitOfWork.Setup(x => x.UserRepository.GetSingleAsync(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(() => null);

            // init controller
            ILogicUnitOfWork logicUnitOfWork = new LogicUnitOfWork(
                mockEntityUnitOfWork.Object,
                mockDapperUnitOfWork.Object,
                mockAuthenticationUnitOfWork,
                mockS3UnitOfWork.Object,
                mockMailUnitOfWork.Object
            );
            var authenticationController = new AuthenticationController(logicUnitOfWork);

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

            // mock db repository &service
            var mockEntityUnitOfWork = new Mock<IEntityUnitOfWork>();
            mockEntityUnitOfWork.Setup(x => x.UserRepository.GetSingleAsync(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(() => mockExistUser);

            // init controller
            ILogicUnitOfWork logicUnitOfWork = new LogicUnitOfWork(
                mockEntityUnitOfWork.Object,
                mockDapperUnitOfWork.Object,
                mockAuthenticationUnitOfWork,
                mockS3UnitOfWork.Object,
                mockMailUnitOfWork.Object
            );
            var authenticationController = new AuthenticationController(logicUnitOfWork);

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
