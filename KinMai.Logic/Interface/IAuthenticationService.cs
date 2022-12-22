using System;
using KinMai.Authentication.Model;

namespace KinMai.Logic.Interface
{
    public interface IAuthenticationService
    {
        Task<TokenResponseModel> Login(string email, string password);
    }
}

