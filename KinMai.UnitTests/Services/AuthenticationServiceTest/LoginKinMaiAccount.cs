using KinMai.Authentication.UnitOfWork;
using KinMai.Dapper.Interface;
using KinMai.EntityFramework.Models;
using KinMai.EntityFramework.UnitOfWork.Implement;
using KinMai.EntityFramework.UnitOfWork.Interface;
using KinMai.Logic.Interface;
using KinMai.Logic.Services;
using KinMai.Mail.UnitOfWork;
using KinMai.S3.UnitOfWork.Interface;
using KinMai.UnitTests.Shared;
using Moq;
using Xunit.Abstractions;

namespace KinMai.UnitTests.Services.AuthenticationServiceTest
{
    public class LoginKinMaiAccount
    {
        private readonly ITestOutputHelper output;
        private readonly InitConfiguration initConfiguration;

        public LoginKinMaiAccount(ITestOutputHelper output)
        {
            this.output = output;
            var initConfiguration = new InitConfiguration();
        }

        [Fact(DisplayName = "Login_ReturnTokenModel_LoginByExistUser")]
        public async Task TestLogin_ReturnTokenModel_LoginByExistUser()
        {
            // mock data
            Guid userId;
            Guid.TryParse("9c16fe15-f21e-4071-94e8-c982b6c9c626", out userId);
            var password = "12345678";

            // mock unit of work
            var mockDapperUnitOfWork = new Mock<IDapperUnitOfWork>();
            var mockS3UnitOfWork = new Mock<IS3UnitOfWork>();
            var mockMailUnitOfWork = new Mock<IMailUnitOfWork>();

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

            using (var context = new KinMaiContext(NewDbContextService.CreateNewContextOptions()))
            {
                // add user to mock db
                context.Users.Add(mockUser);
                context.SaveChanges();

                IEntityUnitOfWork mockEntityUnitOfWork = new EntityUnitOfWork(context);
                IAuthenticationUnitOfWork mockAuthenticationUnitOfWork = new AuthenticationUnitOfWork();

                // init service
                IAuthenticationService authenticationService =
                    new AuthenticationService(
                        mockEntityUnitOfWork,
                        mockAuthenticationUnitOfWork,
                        mockDapperUnitOfWork.Object,
                        mockS3UnitOfWork.Object,
                        mockMailUnitOfWork.Object
                   );

                // act
                var actualOutput = await authenticationService.Login(mockUser.Email, password);

                // assert
                Assert.NotNull(actualOutput);
            }
        }

        [Fact(DisplayName = "Login_ThrowArgumentException_LoginByIncorrectPassword")]
        public async Task TestLogin_ThrowArgumentException_LoginByIncorrectPassword()
        {
            // mock data
            Guid userId;
            Guid.TryParse("9c16fe15-f21e-4071-94e8-c982b6c9c626", out userId);
            var password = "11111111";

            // mock unit of work
            var mockDapperUnitOfWork = new Mock<IDapperUnitOfWork>();
            var mockS3UnitOfWork = new Mock<IS3UnitOfWork>();
            var mockMailUnitOfWork = new Mock<IMailUnitOfWork>();

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

            using (var context = new KinMaiContext(NewDbContextService.CreateNewContextOptions()))
            {
                // add user to mock db
                context.Users.Add(mockUser);
                context.SaveChanges();

                IEntityUnitOfWork mockEntityUnitOfWork = new EntityUnitOfWork(context);
                IAuthenticationUnitOfWork mockAuthenticationUnitOfWork = new AuthenticationUnitOfWork();

                // init service
                IAuthenticationService authenticationService =
                    new AuthenticationService(
                        mockEntityUnitOfWork,
                        mockAuthenticationUnitOfWork,
                        mockDapperUnitOfWork.Object,
                        mockS3UnitOfWork.Object,
                        mockMailUnitOfWork.Object
                   );

                // act
                Func<Task> act = () => authenticationService.Login(mockUser.Email, password);

                // assert
                var exception = await Assert.ThrowsAsync<Exception>(act);
                Assert.Equal("Invalid Email or password.", exception.Message);
            }
        }

        [Fact(DisplayName = "Login_ThrowArgumentException_LoginByNewUser")]
        public async Task TestLogin_ThrowArgumentException_LoginByNewUser()
        {
            // mock data
            var email = "nampunch1@gmail.com";
            var password = "12345678";

            // mock unit of work
            var mockDapperUnitOfWork = new Mock<IDapperUnitOfWork>();
            var mockS3UnitOfWork = new Mock<IS3UnitOfWork>();
            var mockMailUnitOfWork = new Mock<IMailUnitOfWork>();

            using (var context = new KinMaiContext(NewDbContextService.CreateNewContextOptions()))
            {
                IEntityUnitOfWork mockEntityUnitOfWork = new EntityUnitOfWork(context);
                IAuthenticationUnitOfWork mockAuthenticationUnitOfWork = new AuthenticationUnitOfWork();

                // init service
                IAuthenticationService authenticationService =
                    new AuthenticationService(
                        mockEntityUnitOfWork,
                        mockAuthenticationUnitOfWork,
                        mockDapperUnitOfWork.Object,
                        mockS3UnitOfWork.Object,
                        mockMailUnitOfWork.Object
                   );

                // act
                Func<Task> act = () => authenticationService.Login(email, password);

                // assert
                var exception = await Assert.ThrowsAsync<ArgumentException>(act);
                Assert.Equal("Email does not exist.", exception.Message);
            }
        }

        [Fact(DisplayName = "Login_ThrowArgumentException_LoginByUserRegisteredByGoogleAccount")]
        public async Task TestLogin_ThrowArgumentException_LoginByUserRegisteredByGoogleAccount()
        {
            // mock data
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

            // mock unit of work
            var mockDapperUnitOfWork = new Mock<IDapperUnitOfWork>();
            var mockS3UnitOfWork = new Mock<IS3UnitOfWork>();
            var mockMailUnitOfWork = new Mock<IMailUnitOfWork>();

            using (var context = new KinMaiContext(NewDbContextService.CreateNewContextOptions()))
            {
                // add user to mock db
                context.Users.Add(mockUser);
                context.SaveChanges();

                IEntityUnitOfWork mockEntityUnitOfWork = new EntityUnitOfWork(context);
                IAuthenticationUnitOfWork mockAuthenticationUnitOfWork = new AuthenticationUnitOfWork();

                // init service
                IAuthenticationService authenticationService =
                    new AuthenticationService(
                        mockEntityUnitOfWork,
                        mockAuthenticationUnitOfWork,
                        mockDapperUnitOfWork.Object,
                        mockS3UnitOfWork.Object,
                        mockMailUnitOfWork.Object
                   );

                // act
                Func<Task> act = () => authenticationService.Login(mockUser.Email, password);

                // assert
                var exception = await Assert.ThrowsAsync<ArgumentException>(act);
                Assert.Equal("This email is registered by Google provider, Please login by Google instead", exception.Message);
            }
        }
    }   
}
