using Amazon.CognitoIdentityProvider.Model;
using KinMai.Authentication.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinMai.Authentication.Interface
{
    public interface IAWSCognitoService
    {
        Task<SignUpResponse> SignUp(Guid userName, string email, string password);
        Task<bool> ConfirmSignUp(Guid username);
        Task<InitiateAuthResponse> Login(Guid username, string password);
        Task<ChangePasswordResponse> ChangePassword(Guid username, string oldPassword, string newPassword);
        Task<bool> ResetPassword(Guid username, string password);
        Task<InitiateAuthResponse> RefreshToken(Guid username, string refreshToken);
    }
}
