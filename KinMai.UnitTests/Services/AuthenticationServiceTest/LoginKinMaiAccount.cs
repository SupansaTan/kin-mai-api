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

        [Fact(DisplayName = "LoginKinMaiAccount_ReturnTokenModel_ByExistUser")]
        public async Task TestLoginKinMaiAccount_ReturnTokenModel_ByExistUser()
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
    }   
}
