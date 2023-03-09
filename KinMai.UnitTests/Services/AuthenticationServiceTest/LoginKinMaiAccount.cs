using KinMai.Authentication.Model;
using KinMai.Authentication.UnitOfWork;
using KinMai.Dapper.Interface;
using KinMai.EntityFramework.Models;
using KinMai.EntityFramework.UnitOfWork.Implement;
using KinMai.EntityFramework.UnitOfWork.Interface;
using KinMai.Logic.Interface;
using KinMai.Logic.Services;
using KinMai.Mail.UnitOfWork;
using KinMai.S3.UnitOfWork.Implement;
using KinMai.S3.UnitOfWork.Interface;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinMai.UnitTests.Services.AuthenticationServiceTest
{
    public class LoginKinMaiAccount
    {
        [Fact(DisplayName = "Login as kinmai account: ReturnTokenModelIfValidModel")]
        public async Task TestLoginKinMaiAccount()
        {
            // use in-memory database for testing
            var builder = new DbContextOptionsBuilder<KinMaiContext>().UseInMemoryDatabase(Guid.NewGuid().ToString());
            var dbContext = new KinMaiContext(builder.Options);

            // mock data
            Guid userId;
            Guid.TryParse("9c16fe15-f21e-4071-94e8-c982b6c9c626", out userId);
            var email = "nampunch1@gmail.com";
            var password = "12345678";

            // mock unit of work
            IEntityUnitOfWork mockEntityUnitOfWork = new EntityUnitOfWork(dbContext);
            var mockAuthenticationUnitOfWork = new Mock<IAuthenticationUnitOfWork>();
            var mockDapperUnitOfWork = new Mock<IDapperUnitOfWork>();
            var mockS3UnitOfWork = new Mock<IS3UnitOfWork>();
            var mockMailUnitOfWork = new Mock<IMailUnitOfWork>();

            // init service
            IAuthenticationService authenticationService = 
                new AuthenticationService(
                    mockEntityUnitOfWork,
                    mockAuthenticationUnitOfWork.Object,
                    mockDapperUnitOfWork.Object,
                    mockS3UnitOfWork.Object,
                    mockMailUnitOfWork.Object
               );

            // act
            var actualOutput = await authenticationService.Login(email, password);
            var token = await mockAuthenticationUnitOfWork.Object.AWSCognitoService.Login(userId, password);
            var expectedOutput = new TokenResponseModel
            {
                Token = token.AuthenticationResult.AccessToken,
                ExpiredToken = (DateTime.UtcNow).AddSeconds(token.AuthenticationResult.ExpiresIn),
                RefreshToken = token.AuthenticationResult.RefreshToken
            };

            // assert
            Assert.Equal(expectedOutput, actualOutput);
        }
    }
}
