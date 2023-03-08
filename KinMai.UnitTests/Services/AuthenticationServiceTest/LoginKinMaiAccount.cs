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
        [Fact(DisplayName = "Login as kinmai account: ReturnTokenIfValidModel")]
        public async Task TestLoginKinMaiAccount()
        {
            // use in-memory database for testing
            var builder = new DbContextOptionsBuilder<KinMaiContext>().UseInMemoryDatabase(Guid.NewGuid().ToString());
            var dbContext = new KinMaiContext(builder.Options);

            // mock data
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
            var expectedOutput = "test";

            // assert
            Assert.Equal(expectedOutput, actualOutput.Token);
        }
    }
}
