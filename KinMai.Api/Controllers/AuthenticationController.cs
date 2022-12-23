using KinMai.Api.Models;
using KinMai.Authentication.Model;
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
    }
}
