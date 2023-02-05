using KinMai.Api.Models;
using KinMai.Api.Models.Reviewer;
using KinMai.Logic.Models;
using KinMai.Logic.UnitOfWork.Interface;
using Microsoft.AspNetCore.Mvc;

namespace KinMai.Api.Controllers
{
    public class ReviewerController : ControllerBase
    {
        private readonly ILogicUnitOfWork _logicUnitOfWork;
        public ReviewerController(ILogicUnitOfWork logicUnitOfWork)
        {
            _logicUnitOfWork = logicUnitOfWork;
        }

        [HttpGet("GetRestaurantNearMeList")]
        public async Task<ResponseModel<RestaurantInfoListModel>> GetRestaurantNearMeList([FromQuery] GetRestaurantNearMeRequestModel request)
        {
            var response = new ResponseModel<RestaurantInfoListModel>();
            try
            {
                var restaurantInfoList = await _logicUnitOfWork.ReviewerService.GetRestaurantNearMeList(request);
                response = new ResponseModel<RestaurantInfoListModel>
                {
                    Data = restaurantInfoList,
                    Message = "success",
                    Status = 200
                };
            }
            catch (ArgumentException ae)
            {
                response = new ResponseModel<RestaurantInfoListModel>
                {
                    Data = null,
                    Message = ae.Message,
                    Status = 400
                };
            }
            catch (Exception e)
            {
                response = new ResponseModel<RestaurantInfoListModel>
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
