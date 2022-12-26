using System;
using System.Net;
using System.Text;
using KinMai.Api.Models;
using KinMai.Authentication.Model;
using KinMai.Authentication.UnitOfWork;
using KinMai.Common.Enum;
using KinMai.Dapper.Interface;
using KinMai.EntityFramework.Models;
using KinMai.EntityFramework.UnitOfWork.Interface;
using KinMai.Logic.Interface;
using KinMai.Logic.Models;
using Newtonsoft.Json;

namespace KinMai.Logic.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IEntityUnitOfWork _entityUnitOfWork;
        private readonly IDapperUnitOfWork _dapperUnitOfWork;
        private readonly IAuthenticationUnitOfWork _authenticationUnitOfWork;
        private readonly string QUERY_PATH;

        public AuthenticationService(
            IEntityUnitOfWork entityUnitOfWork,
            IAuthenticationUnitOfWork authenticationUnitOfWork,
            IDapperUnitOfWork dapperUnitOfWork)
        {
            QUERY_PATH = this.GetType().Name.Split("Service")[0] + "/";
            _entityUnitOfWork = entityUnitOfWork;
            _authenticationUnitOfWork = authenticationUnitOfWork;
            _dapperUnitOfWork = dapperUnitOfWork;
        }
        public async Task<TokenResponseModel> Login(string email, string password)
        {
            // validate user
            var user = await _entityUnitOfWork.UserRepository.GetSingleAsync(x => x.Email.ToLower() == email.ToLower());
            if (user == null)
                throw new ArgumentException("Email does not exist.");

            // validate auth
            var access = await _authenticationUnitOfWork.AWSCognitoService.Login(user.Id, password);
            if (access.HttpStatusCode != HttpStatusCode.OK)
                throw new ArgumentException("Invalid Email or password.");

            return new TokenResponseModel
            {
                Token = access.AuthenticationResult.AccessToken,
                ExpiredToken = (DateTime.UtcNow).AddSeconds(access.AuthenticationResult.ExpiresIn),
                RefreshToken = access.AuthenticationResult.RefreshToken
            };
        }
        public async Task<bool> ReviewerRegister(ReviewerRegisterModel model)
        {
            // validate
            var user = await _entityUnitOfWork.UserRepository.GetSingleAsync(x => x.Email.ToLower() == model.Email.ToLower());
            if (user != null) throw new ArgumentException("Email already exists.");
            if (model.Password != model.ConfirmPassword) throw new ArgumentException("Password and Confirm password are not matching");

            // create user
            user = new User
            {
                Id = Guid.NewGuid(),
                Email = model.Email.ToLower(),
                CreateAt = DateTime.UtcNow,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Username = model.Username,
                UserType = (int)UserType.Reviewer
            };

            var singup = await _authenticationUnitOfWork.AWSCognitoService.SignUp(user.Id, user.Email, model.Password);
            if (singup.HttpStatusCode != HttpStatusCode.OK)
                throw new ArgumentException("Can't register, Please contact admin.");

            var confirmSignup = await _authenticationUnitOfWork.AWSCognitoService.ConfirmSignUp(user.Id);
            if (!confirmSignup)
                throw new ArgumentException("Can't confirmed register, Please try again.");

            _entityUnitOfWork.UserRepository.Add(user);
            await _entityUnitOfWork.SaveAsync();
            return true;
        }
    }
}

