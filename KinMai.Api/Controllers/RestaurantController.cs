using KinMai.Api.Models;
using KinMai.Logic.Models;
using KinMai.Logic.UnitOfWork.Interface;
using Microsoft.AspNetCore.Mvc;

namespace KinMai.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RestaurantController : ControllerBase
    {
        private readonly ILogicUnitOfWork _logicUnitOfWork;
        public RestaurantController(ILogicUnitOfWork logicUnitOfWork)
        {
            _logicUnitOfWork = logicUnitOfWork;
        }
        [HttpGet("GetRestaurantDetail")]
        public async Task<ResponseModel<RestaurantDetailModel>> GetRestaurantDetail([FromQuery] GetReviewInfoRequest request)
        {
            var response = new ResponseModel<RestaurantDetailModel>();
            try
            {
                var payload = await _logicUnitOfWork.RestaurantService.GetRestaurantDetail(request);
                if (payload != null)
                {
                    response = new ResponseModel<RestaurantDetailModel>
                    {
                        Data = payload,
                        Message = "success",
                        Status = 200
                    };
                }
            }
            catch (ArgumentException ae)
            {
                response = new ResponseModel<RestaurantDetailModel>
                {
                    Data = null,
                    Message = ae.Message,
                    Status = 400
                };
            }
            catch (Exception e)
            {
                response = new ResponseModel<RestaurantDetailModel>
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
