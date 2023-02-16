using KinMai.Api.Models;
using KinMai.Api.Models.Reviewer;
using KinMai.Logic.Models;
using KinMai.Logic.UnitOfWork.Interface;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;

namespace KinMai.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
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
        [HttpGet("GetRestaurantListFromFilter")]
        public async Task<ResponseModel<RestaurantCardListModel>> GetRestaurantListFromFilter([FromQuery] GetRestaurantListFromFilterRequestModel request)
        {
            var response = new ResponseModel<RestaurantCardListModel>();
            try
            {
                var restaurantInfoList = await _logicUnitOfWork.ReviewerService.GetRestaurantListFromFilter(request);
                response = new ResponseModel<RestaurantCardListModel>
                {
                    Data = restaurantInfoList,
                    Message = "success",
                    Status = 200
                };
            }
            catch (ArgumentException ae)
            {
                response = new ResponseModel<RestaurantCardListModel>
                {
                    Data = null,
                    Message = ae.Message,
                    Status = 400
                };
            }
            catch (Exception e)
            {
                response = new ResponseModel<RestaurantCardListModel>
                {
                    Data = null,
                    Message = e.Message,
                    Status = 500
                };
            }
            return response;
        }
        [HttpPost("SetFavoriteRestaurant")]
        public async Task<ResponseModel<bool>> SetFavoriteRestaurant([FromBody] SetFavoriteResturantRequestModel request)
        {
            var response = new ResponseModel<bool>();
            try
            {
                var isSuccess = await _logicUnitOfWork.ReviewerService.SetFavoriteRestaurant(request);
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

        [HttpPost("AddReviewRestaurant")]
        public async Task<ResponseModel<bool>> AddReviewRestaurant([FromForm] AddReviewRequestModel request)
        {
            var response = new ResponseModel<bool>();
            try
            {
                var isSuccess = await _logicUnitOfWork.ReviewerService.AddReviewRestaurant(request);
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
    }
}
