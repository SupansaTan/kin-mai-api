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
    public class GetUserInfo
    {
        private readonly InitConfiguration initConfiguration;
        private readonly Mock<IDapperUnitOfWork> mockDapperUnitOfWork;
        private readonly Mock<IS3UnitOfWork> mockS3UnitOfWork;
        private readonly Mock<IMailUnitOfWork> mockMailUnitOfWork;
        private readonly Mock<IAuthenticationUnitOfWork> mockAuthenticationUnitOfWork;
        private readonly Mock<IEntityUnitOfWork> mockEntityUnitOfWork;
        private readonly ILogicUnitOfWork logicUnitOfWork;
        private readonly AuthenticationController authenticationController;

        public GetUserInfo()
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
            var actualOutput = await authenticationController.GetUserInfo("nampunch1@gmail.com");
            var expectOutput = new ResponseModel<UserInfoModel>()
            {
                Data = new UserInfoModel()
                {
                    UserId = mockUser.Id,
                    UserName = mockUser.Username,
                    RestaurantName = "",
                    UserType = (Common.Enum.UserType)mockUser.UserType
                },
                Message = "success",
                Status = 200
            };

            // assert
            Assert.Equal(expectOutput.Data.UserId, actualOutput.Data.UserId);
            Assert.Equal(expectOutput.Data.UserName, actualOutput.Data.UserName);
            Assert.Equal(expectOutput.Data.RestaurantName, actualOutput.Data.RestaurantName);
            Assert.Equal(expectOutput.Data.UserType, actualOutput.Data.UserType);
            Assert.Equal(expectOutput.Message, actualOutput.Message);
            Assert.Equal(expectOutput.Status, actualOutput.Status);
        }

        [Fact]
        public async Task RestaurantRegister_ThrowArgumentException_WhenEmailDoesNotExist()
        {
            // setup database response
            mockEntityUnitOfWork.Setup(x => x.UserRepository.GetSingleAsync(It.IsAny<Expression<Func<User, bool>>>()))
                                .ReturnsAsync(() => null);

            // act
            var actualOutput = await authenticationController.GetUserInfo("nampunch1@gmail.com");
            var expectOutput = new ResponseModel<UserInfoModel>()
            {
                Data = null,
                Message = "Email does not exist.",
                Status = 400
            };

            // assert
            Assert.Equal(expectOutput.Data, actualOutput.Data);
            Assert.Equal(expectOutput.Message, actualOutput.Message);
            Assert.Equal(expectOutput.Status, actualOutput.Status);
        }
    }
}
