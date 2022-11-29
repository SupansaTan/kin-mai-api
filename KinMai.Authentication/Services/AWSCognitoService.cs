using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using KinMai.Authentication.Interface;
using KinMai.Authentication.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
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

        public async Task<SignUpResponse> SignUp(Guid username, string email, string password)
        {
            var request = new SignUpRequest
            {
                ClientId = AWSCredential.ClientId,
                SecretHash = EncodeSecretHash(username.ToString()),
                Username = username.ToString(),
                Password = password
            };

            if (!string.IsNullOrWhiteSpace(email))
            {
                request.UserAttributes.Add(new AttributeType()
                {
                    Name = "email",
                    Value = email
                });
            }
            return await amazonCognito.SignUpAsync(request);
        }
        public async Task<InitiateAuthResponse> Login(Guid username, string password)
        {
            try
            {
                string clientSecret = await GetClientSecret();
                return await InitiateAuth(username.ToString(), password);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message, e);
            }
        }
        public async Task<ChangePasswordResponse> ChangePassword(Guid username, string oldPassword, string newPassword)
        {
            var authResponse = await InitiateAuth(username.ToString(), oldPassword);

            return await amazonCognito.ChangePasswordAsync(new ChangePasswordRequest
            {
                AccessToken = authResponse.AuthenticationResult.AccessToken,
                PreviousPassword = oldPassword,
                ProposedPassword = newPassword
            });
        }
        public async Task<bool> ResetPassword(Guid username, string password)
        {
            var data = await amazonCognito.AdminSetUserPasswordAsync(new AdminSetUserPasswordRequest()
            {
                Username = username.ToString(),
                Password = password,
                Permanent = true,
                UserPoolId = AWSCredential.PoolId
            }).ConfigureAwait(false);

            return data.HttpStatusCode == HttpStatusCode.OK;
        }
        public async Task<InitiateAuthResponse> RefreshToken(Guid username, string refreshToken)
        {
            var request = new InitiateAuthRequest
            {
                ClientId = AWSCredential.ClientId,
                AuthFlow = AuthFlowType.REFRESH_TOKEN_AUTH
            };

            request.AuthParameters.Add("REFRESH_TOKEN", refreshToken);
            request.AuthParameters.Add("SECRET_HASH", EncodeSecretHash(username.ToString()));
            return await amazonCognito.InitiateAuthAsync(request);
        }
        private async Task<InitiateAuthResponse> InitiateAuth(string username, string password)
        {
            var request = new InitiateAuthRequest
            {
                ClientId = AWSCredential.ClientId,
                AuthFlow = AuthFlowType.USER_PASSWORD_AUTH
            };
            request.AuthParameters.Add("USERNAME", username);
            request.AuthParameters.Add("PASSWORD", password);
            request.AuthParameters.Add("SECRET_HASH", EncodeSecretHash(username));
            return await amazonCognito.InitiateAuthAsync(request);
        }
        private string EncodeSecretHash(string username)
        {
            var dataString = username + "" + AWSCredential.ClientId;
            var data = Encoding.UTF8.GetBytes(dataString);
            var key = Encoding.UTF8.GetBytes(AWSCredential.ClientSecret);

            using (var shaAlgorithm = new HMACSHA256(key))
            {
                var byteHash = shaAlgorithm.ComputeHash(data);
                return Convert.ToBase64String(byteHash);
            }
        }
        private async Task<string> GetClientSecret()
        {
            var describeUserPool = await amazonCognito.DescribeUserPoolClientAsync(new DescribeUserPoolClientRequest()
            {
                ClientId = AWSCredential.ClientId,
                UserPoolId = AWSCredential.PoolId
            }).ConfigureAwait(false);

            return describeUserPool.UserPoolClient.ClientSecret;
        }
    }
}
