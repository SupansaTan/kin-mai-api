using System;
using System.Net;
using KinMai.Authentication.Model;
using KinMai.Authentication.UnitOfWork;
using KinMai.Dapper.Interface;
using KinMai.EntityFramework.UnitOfWork.Interface;
using KinMai.Logic.Interface;

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
    }
}

