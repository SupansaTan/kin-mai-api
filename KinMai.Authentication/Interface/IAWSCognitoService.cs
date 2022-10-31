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
        Task<TokenResponseModel> LoginAsync(string email, string password);
        Task<bool> ChangePasswordAsync(string email, string password);
    }
}
