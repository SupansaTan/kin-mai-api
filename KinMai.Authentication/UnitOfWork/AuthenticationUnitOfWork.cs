using Amazon;
using Amazon.CognitoIdentityProvider;
using KinMai.Authentication.Interface;
using KinMai.Authentication.Model;
using KinMai.Authentication.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinMai.Authentication.UnitOfWork
{
    public class AuthenticationUnitOfWork : IAuthenticationUnitOfWork
    {
        private IAWSCognitoService IAWSCognitoService;
        private readonly AmazonCognitoIdentityProviderClient amazonCognito;

        public AuthenticationUnitOfWork()
        {
            amazonCognito = new AmazonCognitoIdentityProviderClient(AWSCredential.AccessKey, AWSCredential.SecretKey, RegionEndpoint.APSoutheast1);
        }

        public IAWSCognitoService AWSCognitoService
        {
            get { return IAWSCognitoService ?? (IAWSCognitoService = new AWSCognitoService(amazonCognito)); }
            set { IAWSCognitoService = value; }
        }
    }
}
