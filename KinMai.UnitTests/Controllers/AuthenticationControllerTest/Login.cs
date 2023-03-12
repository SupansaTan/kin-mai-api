using KinMai.Api.Controllers;
using KinMai.Api.Models;
using KinMai.Authentication.Model;
using KinMai.Authentication.UnitOfWork;
using KinMai.Dapper.Interface;
using KinMai.EntityFramework.Models;
using KinMai.EntityFramework.UnitOfWork.Implement;
using KinMai.EntityFramework.UnitOfWork.Interface;
using KinMai.Logic.UnitOfWork.Implement;
using KinMai.Logic.UnitOfWork.Interface;
using KinMai.Mail.UnitOfWork;
using KinMai.S3.UnitOfWork.Interface;
using KinMai.UnitTests.Shared;
using Moq;
using Xunit.Abstractions;

namespace KinMai.UnitTests.Controllers.AuthenticationControllerTest
{
    public class Login
    {
        private readonly ITestOutputHelper output;
        private readonly InitConfiguration initConfiguration;

        public Login(ITestOutputHelper output)
        {
            this.output = output;
            var initConfiguration = new InitConfiguration();
        }

        [Fact]
        public async Task Login_ReturnStatus200WithTokenModel_WhenLoginByExistUser()
        {
            // mock data
            Guid userId;
            Guid.TryParse("9c16fe15-f21e-4071-94e8-c982b6c9c626", out userId);

            // mock unit of work
            var mockDapperUnitOfWork = new Mock<IDapperUnitOfWork>();
            var mockS3UnitOfWork = new Mock<IS3UnitOfWork>();
            var mockMailUnitOfWork = new Mock<IMailUnitOfWork>();
            IAuthenticationUnitOfWork mockAuthenticationUnitOfWork = new AuthenticationUnitOfWork();

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

            using (var context = new KinMaiContext(NewDbContextService.CreateNewContextOptions()))
            {
                // add user to mock db
                context.Users.Add(mockUser);
                context.SaveChanges();

                // init controller
                IEntityUnitOfWork mockEntityUnitOfWork = new EntityUnitOfWork(context);
                ILogicUnitOfWork logicUnitOfWork = new LogicUnitOfWork(
                    mockEntityUnitOfWork,
                    mockDapperUnitOfWork.Object,
                    mockAuthenticationUnitOfWork,
                    mockS3UnitOfWork.Object,
                    mockMailUnitOfWork.Object
                );
                var authenticationController = new AuthenticationController(logicUnitOfWork);

                // act
                var actualOutput = await authenticationController.Login(mockRequest);
                var expectOutput = new ResponseModel<TokenResponseModel>()
                {
                    Data = actualOutput.Data,
                    Message = "success",
                    Status = 200
                };

                // assert
                Assert.Equal(actualOutput.Message, expectOutput.Message);
                Assert.Equal(actualOutput.Status, expectOutput.Status);
            }
        }

        [Fact]
        public async Task Login_ReturnStatus500_WhenLoginWithIncorrectPassword()
        {
            // mock data
            Guid userId;
            Guid.TryParse("9c16fe15-f21e-4071-94e8-c982b6c9c626", out userId);

            // mock unit of work
            var mockDapperUnitOfWork = new Mock<IDapperUnitOfWork>();
            var mockS3UnitOfWork = new Mock<IS3UnitOfWork>();
            var mockMailUnitOfWork = new Mock<IMailUnitOfWork>();
            IAuthenticationUnitOfWork mockAuthenticationUnitOfWork = new AuthenticationUnitOfWork();

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

            using (var context = new KinMaiContext(NewDbContextService.CreateNewContextOptions()))
            {
                // add user to mock db
                context.Users.Add(mockUser);
                context.SaveChanges();

                // init controller
                IEntityUnitOfWork mockEntityUnitOfWork = new EntityUnitOfWork(context);
                ILogicUnitOfWork logicUnitOfWork = new LogicUnitOfWork(
                    mockEntityUnitOfWork,
                    mockDapperUnitOfWork.Object,
                    mockAuthenticationUnitOfWork,
                    mockS3UnitOfWork.Object,
                    mockMailUnitOfWork.Object
                );
                var authenticationController = new AuthenticationController(logicUnitOfWork);

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

            // mock unit of work
            var mockDapperUnitOfWork = new Mock<IDapperUnitOfWork>();
            var mockS3UnitOfWork = new Mock<IS3UnitOfWork>();
            var mockMailUnitOfWork = new Mock<IMailUnitOfWork>();
            IAuthenticationUnitOfWork mockAuthenticationUnitOfWork = new AuthenticationUnitOfWork();

            using (var context = new KinMaiContext(NewDbContextService.CreateNewContextOptions()))
            {
                // init controller
                IEntityUnitOfWork mockEntityUnitOfWork = new EntityUnitOfWork(context);
                ILogicUnitOfWork logicUnitOfWork = new LogicUnitOfWork(
                    mockEntityUnitOfWork,
                    mockDapperUnitOfWork.Object,
                    mockAuthenticationUnitOfWork,
                    mockS3UnitOfWork.Object,
                    mockMailUnitOfWork.Object
                );
                var authenticationController = new AuthenticationController(logicUnitOfWork);

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
        }
    }
}
