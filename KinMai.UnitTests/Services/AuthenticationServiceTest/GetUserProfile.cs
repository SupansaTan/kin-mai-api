﻿using KinMai.Authentication.UnitOfWork;
using KinMai.Dapper.Interface;
using KinMai.EntityFramework.Models;
using KinMai.EntityFramework.UnitOfWork.Interface;
using KinMai.Logic.Interface;
using KinMai.Logic.Models;
using KinMai.Logic.Services;
using KinMai.Mail.UnitOfWork;
using KinMai.S3.UnitOfWork.Interface;
using KinMai.UnitTests.Shared;
using Moq;
using System.Linq.Expressions;

namespace KinMai.UnitTests.Services.AuthenticationServiceTest
{
    public class GetUserProfile
    {
        private readonly InitConfiguration initConfiguration;
        private Mock<IDapperUnitOfWork> mockDapperUnitOfWork;
        private Mock<IS3UnitOfWork> mockS3UnitOfWork;
        private Mock<IMailUnitOfWork> mockMailUnitOfWork;
        private Mock<IAuthenticationUnitOfWork> mockAuthenticationUnitOfWork;
        private Mock<IEntityUnitOfWork> mockEntityUnitOfWork;
        private IAuthenticationService authenticationService;

        public GetUserProfile()
        {
            initConfiguration = new InitConfiguration();
            mockDapperUnitOfWork = new Mock<IDapperUnitOfWork>();
            mockS3UnitOfWork = new Mock<IS3UnitOfWork>();
            mockMailUnitOfWork = new Mock<IMailUnitOfWork>();
            mockAuthenticationUnitOfWork = new Mock<IAuthenticationUnitOfWork>();
            mockEntityUnitOfWork = new Mock<IEntityUnitOfWork>();
            authenticationService = new AuthenticationService(
                mockEntityUnitOfWork.Object,
                mockAuthenticationUnitOfWork.Object,
                mockDapperUnitOfWork.Object,
                mockS3UnitOfWork.Object,
                mockMailUnitOfWork.Object
            );
        }

        [Fact]
        public async Task GetUserProfile_ReturnGetUserProfileModel_WhenUserIsExist()
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
            var actualOutput = await authenticationService.GetUserProfile(mockUser.Id);
            var expectedOutput = new GetUserProfileModel()
            {
                UserId = mockUser.Id,
                FirstName = "Supansa",
                LastName = "Tantulset",
                Username = "littlepunchhz",
                Email = "nampunch1@gmail.com",
                IsLoginWithGoogle = false
            };

            // assert
            mockEntityUnitOfWork.VerifyAll();
            Assert.Equal(expectedOutput.UserId, actualOutput.UserId);
            Assert.Equal(expectedOutput.FirstName, actualOutput.FirstName);
            Assert.Equal(expectedOutput.LastName, actualOutput.LastName);
            Assert.Equal(expectedOutput.Username, actualOutput.Username);
            Assert.Equal(expectedOutput.Email, actualOutput.Email);
            Assert.Equal(expectedOutput.IsLoginWithGoogle, actualOutput.IsLoginWithGoogle);
        }

        [Fact]
        public async Task GetUserProfile_ThrowArgumentException_WhenUserDoesNotExist()
        {
            // setup database response
            mockEntityUnitOfWork.Setup(x => x.UserRepository.GetSingleAsync(It.IsAny<Expression<Func<User, bool>>>()))
                                .ReturnsAsync(() => null);

            // act
            Func<Task> act = () => authenticationService.GetUserProfile(Guid.NewGuid());

            // assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(act);
            mockEntityUnitOfWork.VerifyAll();
            Assert.Equal("User does not exists.", exception.Message);
        }
    }
}
