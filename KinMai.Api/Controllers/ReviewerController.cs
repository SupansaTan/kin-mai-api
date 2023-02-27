using KinMai.Api.Models;
using KinMai.Api.Models.Reviewer;
using KinMai.Logic.Models;
using KinMai.Logic.UnitOfWork.Interface;
using Microsoft.AspNetCore.Mvc;

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
        [HttpGet("GetReviewInfo")]
        public async Task<ResponseModel<ReviewInfoModel>> GetReviewInfo([FromQuery] GetReviewInfoRequest request)
        {
            var response = new ResponseModel<ReviewInfoModel>();
            try
            {
                var reviewInfo = await _logicUnitOfWork.ReviewerService.GetReviewInfo(request);
                response = new ResponseModel<ReviewInfoModel>
                {
                    Data = reviewInfo,
                    Message = "success",
                    Status = 200
                };
            }
            catch (ArgumentException ae)
            {
                response = new ResponseModel<ReviewInfoModel>
                {
                    Data = null,
                    Message = ae.Message,
                    Status = 400
                };
            }
            catch (Exception e)
            {
                response = new ResponseModel<ReviewInfoModel>
                {
                    Data = null,
                    Message = e.Message,
                    Status = 500
                };
            }
            return response;
        }
        [HttpPut("UpdateReviewInfo")]
        public async Task<ResponseModel<bool>> UpdateReviewInfo([FromForm] UpdateReviewInfoRequest request)
        {
            var response = new ResponseModel<bool>();
            try
            {
                var updateSuccess = await _logicUnitOfWork.ReviewerService.UpdateReviewInfo(request);
                response = new ResponseModel<bool>
                {
                    Data = updateSuccess,
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
        [HttpGet("GetRestaurantDetail")]
        public async Task<ResponseModel<GetRestaurantDetailModel>> GetRestaurantDetail([FromQuery] GetRestaurantDetailRequestModel request)
        {
            var response = new ResponseModel<GetRestaurantDetailModel>();
            try
            {
                var restaurantDetail = await _logicUnitOfWork.ReviewerService.GetRestaurantDetail(request);
                response = new ResponseModel<GetRestaurantDetailModel>
                {
                    Data = restaurantDetail,
                    Message = "success",
                    Status = 200
                };
            }
            catch (ArgumentException ae)
            {
                response = new ResponseModel<GetRestaurantDetailModel>
                {
                    Data = null,
                    Message = ae.Message,
                    Status = 400
                };
            }
            catch (Exception e)
            {
                response = new ResponseModel<GetRestaurantDetailModel>
                {
                    Data = null,
                    Message = e.Message,
                    Status = 500
                };
            }
            return response;
        }
        [HttpGet("GetRestaurantReviewList")]
        public async Task<ResponseModel<GetReviewInfoListModel>> GetRestaurantReviewList([FromQuery] GetReviewInfoFilterModel request)
        {
            var response = new ResponseModel<GetReviewInfoListModel>();
            try
            {
                var reviewInfo = await _logicUnitOfWork.ReviewerService.GetRestaurantReviewList(request);
                response = new ResponseModel<GetReviewInfoListModel>
                {
                    Data = reviewInfo,
                    Message = "success",
                    Status = 200
                };
            }
            catch (ArgumentException ae)
            {
                response = new ResponseModel<GetReviewInfoListModel>
                {
                    Data = null,
                    Message = ae.Message,
                    Status = 400
                };
            }
            catch (Exception e)
            {
                response = new ResponseModel<GetReviewInfoListModel>
                {
                    Data = null,
                    Message = e.Message,
                    Status = 500
                };
            }
            return response;
        }
        [HttpGet("GetFavoriteRestaurantList")]
        public async Task<ResponseModel<List<GetFavoriteRestaurantList>>> GetFavoriteRestaurantList([FromQuery] GetFavoriteRestaurantRequest request)
        {
            var response = new ResponseModel<List<GetFavoriteRestaurantList>>();
            try
            {
                var restaurantList = await _logicUnitOfWork.ReviewerService.GetFavoriteRestaurantList(request);
                response = new ResponseModel<List<GetFavoriteRestaurantList>>
                {
                    Data = restaurantList,
                    Message = "success",
                    Status = 200
                };
            }
            catch (ArgumentException ae)
            {
                response = new ResponseModel<List<GetFavoriteRestaurantList>>
                {
                    Data = null,
                    Message = ae.Message,
                    Status = 400
                };
            }
            catch (Exception e)
            {
                response = new ResponseModel<List<GetFavoriteRestaurantList>>
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
