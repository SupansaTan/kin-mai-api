using KinMai.Api.Models;
using KinMai.Authentication.Model;
using KinMai.EntityFramework.Models;
using KinMai.Logic.Models;
using KinMai.Logic.UnitOfWork.Interface;
using Microsoft.AspNetCore.Mvc;

namespace KinMai.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly ILogicUnitOfWork _logicUnitOfWork;

        public AuthenticationController(ILogicUnitOfWork logicUnitOfWork)
        {
            _logicUnitOfWork = logicUnitOfWork;
        }
        [HttpPost("Login")]
        public async Task<ResponseModel<TokenResponseModel>> Login([FromBody] LoginModel request)
        {
            var response = new ResponseModel<TokenResponseModel>();
            try
            {
                var payload = await _logicUnitOfWork.AuthenticationService.Login(request.Email, request.Password);
                if (payload != null)
                {
                    response = new ResponseModel<TokenResponseModel>
                    {
                        Data = new TokenResponseModel
                        {
                            ExpiredToken = payload.ExpiredToken,
                            RefreshToken = payload.RefreshToken,
                            Token = payload.Token,
                        },
                        Message = "success",
                        Status = 200
                    };
                }
            }
            catch (ArgumentException ae)
            {
                response = new ResponseModel<TokenResponseModel>
                {
                    Data = null,
                    Message = ae.Message,
                    Status = 400
                };
            }
            catch (Exception e)
            {
                response = new ResponseModel<TokenResponseModel>
                {
                    Data = null,
                    Message = e.Message,
                    Status = 500
                };
            }
            return response;
        }
        [HttpPost("ReviewerRegister")]
        public async Task<ResponseModel<bool>> ReviewerRegister(ReviewerRegisterModel request)
        {
            var response = new ResponseModel<bool>();
            try
            {
                var registerStatus = await _logicUnitOfWork.AuthenticationService.ReviewerRegister(request);
                response = new ResponseModel<bool>
                {
                    Data = registerStatus,
                    Message = "success",
                    Status = 200
                };
            }
            catch (ArgumentException ae)
            {
                response = new ResponseModel<bool>
                {
                    Data = false,
                    Message = ae.Message,
                    Status = 400
                };
            }
            catch (Exception e)
            {
                response = new ResponseModel<bool>
                {
                    Data = false,
                    Message = e.Message,
                    Status = 500
                };
            }
            return response;
        }
        [HttpPost("RestaurantRegister")]
        public async Task<ResponseModel<bool>> RestaurantRegister([FromForm] RestaurantRegisterModel request)
        {
            var response = new ResponseModel<bool>();
            try
            {
                var registerStatus = await _logicUnitOfWork.AuthenticationService.RestaurantRegister(request);
                response = new ResponseModel<bool>
                {
                    Data = registerStatus,
                    Message = "success",
                    Status = 200
                };
            }
            catch (ArgumentException ae)
            {
                response = new ResponseModel<bool>
                {
                    Data = false,
                    Message = ae.Message,
                    Status = 400
                };
            }
            catch (Exception e)
            {
                response = new ResponseModel<bool>
                {
                    Data = false,
                    Message = e.Message,
                    Status = 500
                };
            }
            return response;
        }
        [HttpGet("GetUserInfo")]
        public async Task<ResponseModel<UserInfoModel>> GetUserInfo([FromQuery] string email)
        {
            var response = new ResponseModel<UserInfoModel>();
            try
            {
                var userInfo = await _logicUnitOfWork.AuthenticationService.GetUserInfo(email);
                response = new ResponseModel<UserInfoModel>
                {
                    Data = userInfo,
                    Message = "success",
                    Status = 200
                };
            }
            catch (ArgumentException ae)
            {
                response = new ResponseModel<UserInfoModel>
                {
                    Data = null,
                    Message = ae.Message,
                    Status = 400
                };
            }
            catch (Exception e)
            {
                response = new ResponseModel<UserInfoModel>
                {
                    Data = null,
                    Message = e.Message,
                    Status = 500
                };
            }
            return response;
        }
        [HttpGet("GetUserProfile")]
        public async Task<ResponseModel<GetUserProfileModel>> GetUserProfile([FromQuery] Guid userId)
        {
            var response = new ResponseModel<GetUserProfileModel>();
            try
            {
                var userInfo = await _logicUnitOfWork.AuthenticationService.GetUserProfile(userId);
                response = new ResponseModel<GetUserProfileModel>
                {
                    Data = userInfo,
                    Message = "success",
                    Status = 200
                };
            }
            catch (ArgumentException ae)
            {
                response = new ResponseModel<GetUserProfileModel>
                {
                    Data = null,
                    Message = ae.Message,
                    Status = 400
                };
            }
            catch (Exception e)
            {
                response = new ResponseModel<GetUserProfileModel>
                {
                    Data = null,
                    Message = e.Message,
                    Status = 500
                };
            }
            return response;
        }
        [HttpPut("UpdateUserProfile")]
        public async Task<ResponseModel<bool>> UpdateUserProfile(UpdateUserProfileModel request)
        {
            var response = new ResponseModel<bool>();
            try
            {
                var isSuccess = await _logicUnitOfWork.AuthenticationService.UpdateUserProfile(request);
                response = new ResponseModel<bool>
                {
                    Data = isSuccess,
                    Message = "success",
                    Status = 200
                };
            }
            catch (ArgumentException ae)
            {
                response = new ResponseModel<bool>
                {
                    Data = false,
                    Message = ae.Message,
                    Status = 400
                };
            }
            catch (Exception e)
            {
                response = new ResponseModel<bool>
                {
                    Data = false,
                    Message = e.Message,
                    Status = 500
                };
            }
            return response;
        }
        [HttpGet("CheckIsLoginWithGoogleFirstTimes")]
        public async Task<ResponseModel<bool>> CheckIsLoginWithGoogleFirstTimes([FromQuery] string email)
        {
            var response = new ResponseModel<bool>();
            try
            {
                var isLoginFirstTime = await _logicUnitOfWork.AuthenticationService.CheckIsLoginWithGoogleFirstTimes(email);
                response = new ResponseModel<bool>
                {
                    Data = isLoginFirstTime,
                    Message = "success",
                    Status = 200
                };
            }
            catch (ArgumentException ae)
            {
                response = new ResponseModel<bool>
                {
                    Data = false,
                    Message = ae.Message,
                    Status = 400
                };
            }
            catch (Exception e)
            {
                response = new ResponseModel<bool>
                {
                    Data = false,
                    Message = e.Message,
                    Status = 500
                };
            }
            return response;
        }
    }
}
