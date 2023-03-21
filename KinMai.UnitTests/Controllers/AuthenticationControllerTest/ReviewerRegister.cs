﻿using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.Runtime;
using KinMai.Api.Controllers;
using KinMai.Api.Models;
using KinMai.Authentication.Model;
using KinMai.Authentication.UnitOfWork;
using KinMai.Dapper.Interface;
using KinMai.EntityFramework.Models;
using KinMai.EntityFramework.UnitOfWork.Implement;
using KinMai.EntityFramework.UnitOfWork.Interface;
using KinMai.Logic.Models;
using KinMai.Logic.UnitOfWork.Implement;
using KinMai.Logic.UnitOfWork.Interface;
using KinMai.Mail.UnitOfWork;
using KinMai.S3.UnitOfWork.Interface;
using KinMai.UnitTests.Shared;
using Moq;
using Xunit.Abstractions;

namespace KinMai.UnitTests.Controllers.AuthenticationControllerTest
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
        public async Task ReviewerRegister_ReturnStatus200_WhenRegisterWithValidModel()
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
        }

        [Fact]
        public async Task ReviewerRegister_ReturnStatus400_WhenPasswordDoNotMatch()
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
        }

        [Fact]
        public async Task ReviewerRegister_ReturnStatus400_WhenRegisterWithExistEmail()
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
}
