﻿using System;
using Amazon.CognitoIdentityProvider.Model;
using KinMai.Authentication.Model;
using KinMai.Logic.Models;
using Microsoft.AspNetCore.Http;

namespace KinMai.Logic.Interface
{
    public interface IAuthenticationService
    {
        Task<TokenResponseModel> Login(string email, string password);
        Task<bool> ReviewerRegister(ReviewerRegisterModel model);
        Task<bool> RestaurantRegister(RestaurantRegisterModel model);
        Task<UserInfoModel> GetUserInfo(string email);
        Task<bool> CheckIsLoginWithGoogleFirstTimes(string email);
    }
}
