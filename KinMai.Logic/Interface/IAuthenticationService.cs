using System;
using KinMai.Authentication.Model;
using KinMai.Logic.Models;

namespace KinMai.Logic.Interface
{
    public interface IAuthenticationService
    {
        Task<TokenResponseModel> Login(string email, string password);
        Task<bool> ReviewerRegister(ReviewerRegisterModel model);
        Task<UserInfoModel> GetUserInfo(string email);
    }
}

