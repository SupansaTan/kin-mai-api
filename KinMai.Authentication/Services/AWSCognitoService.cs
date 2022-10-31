using Amazon.CognitoIdentityProvider;
using Amazon.Extensions.CognitoAuthentication;
using KinMai.Authentication.Interface;
using KinMai.Authentication.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinMai.Authentication.Services
{
    public class AWSCognitoService : IAWSCognitoService
    {
        public AWSCognitoService (
            IAmazonCognitoIdentityProvider identityProvider,
            CognitoUserPool userPool
        )
        { }

        public async Task<TokenResponseModel> LoginAsync(string email, string password)
        {
            return new TokenResponseModel()
            {
                Token = "",
                RefreshToken = "",
                ExpiredToken = ""
            };
        }
        public async Task<bool> ChangePasswordAsync(string email, string password)
        {
            return true;
        }
    }
}
