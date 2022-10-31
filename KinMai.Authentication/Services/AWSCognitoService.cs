using Amazon.CognitoIdentityProvider;
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
        private readonly AmazonCognitoIdentityProviderClient amazonCognito;
        public AWSCognitoService (
            AmazonCognitoIdentityProviderClient amazonCognito
        )
        {
            this.amazonCognito = amazonCognito;
        }

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
