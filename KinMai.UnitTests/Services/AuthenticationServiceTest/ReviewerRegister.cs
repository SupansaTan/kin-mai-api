using KinMai.Authentication.UnitOfWork;
using KinMai.Dapper.Interface;
using KinMai.EntityFramework.Models;
using KinMai.EntityFramework.UnitOfWork.Implement;
using KinMai.EntityFramework.UnitOfWork.Interface;
using KinMai.Logic.Interface;
using KinMai.Logic.Models;
using KinMai.Logic.Services;
using KinMai.Mail.UnitOfWork;
using KinMai.S3.UnitOfWork.Interface;
using KinMai.UnitTests.Shared;
using Moq;
using Xunit.Abstractions;

namespace KinMai.UnitTests.Services.AuthenticationServiceTest
{
    public class ReviewerRegister
    {
        private readonly ITestOutputHelper output;
        private readonly InitConfiguration initConfiguration;

        public ReviewerRegister(ITestOutputHelper output)
        {
            this.output = output;
            var initConfiguration = new InitConfiguration();
        }

        [Fact]
        public async Task ReviewerRegister_ReturnTrue_WhenRegisterWithValidModel()
        {
            // mock unit of work
            var mockDapperUnitOfWork = new Mock<IDapperUnitOfWork>();
            var mockS3UnitOfWork = new Mock<IS3UnitOfWork>();
            var mockMailUnitOfWork = new Mock<IMailUnitOfWork>();
            IAuthenticationUnitOfWork mockAuthenticationUnitOfWork = new AuthenticationUnitOfWork();

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

            using (var context = new KinMaiContext(NewDbContextService.CreateNewContextOptions()))
            {
                // init service
                IEntityUnitOfWork mockEntityUnitOfWork = new EntityUnitOfWork(context);
                IAuthenticationService authenticationService =
                    new AuthenticationService(
                        mockEntityUnitOfWork,
                        mockAuthenticationUnitOfWork,
                        mockDapperUnitOfWork.Object,
                        mockS3UnitOfWork.Object,
                        mockMailUnitOfWork.Object
                   );

                // act
                var actualOutput = await authenticationService.ReviewerRegister(mockRequest);

                // assert
                Assert.True(actualOutput);
            }
        }

        [Fact]
        public async Task ReviewerRegister_ReturnTrue_WhenRegisterWithGoogleAccount()
        {
            // mock unit of work
            var mockDapperUnitOfWork = new Mock<IDapperUnitOfWork>();
            var mockS3UnitOfWork = new Mock<IS3UnitOfWork>();
            var mockMailUnitOfWork = new Mock<IMailUnitOfWork>();
            IAuthenticationUnitOfWork mockAuthenticationUnitOfWork = new AuthenticationUnitOfWork();

            // arrange
            var mockRequest = new ReviewerRegisterModel()
            {
                Email = "nampunch1@gmail.com",
                FirstName = "Supansa",
                LastName = "Tantulset",
                Username = "littlepunchhz"
            };

            using (var context = new KinMaiContext(NewDbContextService.CreateNewContextOptions()))
            {
                // init service
                IEntityUnitOfWork mockEntityUnitOfWork = new EntityUnitOfWork(context);
                IAuthenticationService authenticationService =
                    new AuthenticationService(
                        mockEntityUnitOfWork,
                        mockAuthenticationUnitOfWork,
                        mockDapperUnitOfWork.Object,
                        mockS3UnitOfWork.Object,
                        mockMailUnitOfWork.Object
                   );

                // act
                var actualOutput = await authenticationService.ReviewerRegister(mockRequest);

                // assert
                Assert.True(actualOutput);
            }
        }

        [Fact]
        public async Task ReviewerRegister_ThrowArgumentException_WhenPasswordDoNotMatch()
        {
            // mock unit of work
            var mockDapperUnitOfWork = new Mock<IDapperUnitOfWork>();
            var mockS3UnitOfWork = new Mock<IS3UnitOfWork>();
            var mockMailUnitOfWork = new Mock<IMailUnitOfWork>();
            IAuthenticationUnitOfWork mockAuthenticationUnitOfWork = new AuthenticationUnitOfWork();

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

            using (var context = new KinMaiContext(NewDbContextService.CreateNewContextOptions()))
            {
                // init service
                IEntityUnitOfWork mockEntityUnitOfWork = new EntityUnitOfWork(context);
                IAuthenticationService authenticationService =
                    new AuthenticationService(
                        mockEntityUnitOfWork,
                        mockAuthenticationUnitOfWork,
                        mockDapperUnitOfWork.Object,
                        mockS3UnitOfWork.Object,
                        mockMailUnitOfWork.Object
                   );

                // act
                Func<Task> act = () => authenticationService.ReviewerRegister(mockRequest);

                // assert
                var exception = await Assert.ThrowsAsync<ArgumentException>(act);
                Assert.Equal("Password and Confirm password are not matching", exception.Message);
            }
        }

        [Fact]
        public async Task ReviewerRegister_ThrowArgumentException_WhenRegisterWithExistEmail()
        {
            // mock unit of work
            var mockDapperUnitOfWork = new Mock<IDapperUnitOfWork>();
            var mockS3UnitOfWork = new Mock<IS3UnitOfWork>();
            var mockMailUnitOfWork = new Mock<IMailUnitOfWork>();
            IAuthenticationUnitOfWork mockAuthenticationUnitOfWork = new AuthenticationUnitOfWork();

            // arrange
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

            var mockRequest = new ReviewerRegisterModel()
            {
                Email = "nampunch1@gmail.com",
                FirstName = "Supansa",
                LastName = "Tantulset",
                Username = "littlepunchhz",
                Password = "12345678",
                ConfirmPassword = "12345678"
            };

            using (var context = new KinMaiContext(NewDbContextService.CreateNewContextOptions()))
            {
                // add exist user to mock db
                context.Users.Add(mockExistUser);
                context.SaveChanges();

                // init service
                IEntityUnitOfWork mockEntityUnitOfWork = new EntityUnitOfWork(context);
                IAuthenticationService authenticationService =
                    new AuthenticationService(
                        mockEntityUnitOfWork,
                        mockAuthenticationUnitOfWork,
                        mockDapperUnitOfWork.Object,
                        mockS3UnitOfWork.Object,
                        mockMailUnitOfWork.Object
                   );

                // act
                Func<Task> act = () => authenticationService.ReviewerRegister(mockRequest);

                // assert
                var exception = await Assert.ThrowsAsync<ArgumentException>(act);
                Assert.Equal("Email already exists.", exception.Message);
            }
        }
    }
}
