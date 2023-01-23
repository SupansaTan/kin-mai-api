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
        [HttpGet("AllRestaurant")]
        public ResponseModel<List<RestaurantDetailInfoModel>> GetAllRestaurant()
        {
            var response = new ResponseModel<List<RestaurantDetailInfoModel>>();
            try
            {
                var payload = _logicUnitOfWork.RestaurantService.GetAllRestaurant();
                if (payload != null)
                {
                    response = new ResponseModel<List<RestaurantDetailInfoModel>>
                    {
                        Data = payload,
                        Message = "success",
                        Status = 200
                    };
                }
            }
            catch (ArgumentException ae)
            {
                response = new ResponseModel<List<RestaurantDetailInfoModel>>
                {
                    Data = null,
                    Message = ae.Message,
                    Status = 400
                };
            }
            catch (Exception e)
            {
                response = new ResponseModel<List<RestaurantDetailInfoModel>>
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
