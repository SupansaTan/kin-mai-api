﻿using KinMai.Api.Models;
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
        public async Task<ResponseModel<RestaurantDetailModel>> GetRestaurantDetail([FromQuery] Guid restaurantId)
        {
            var response = new ResponseModel<RestaurantDetailModel>();
            try
            {
                var payload = await _logicUnitOfWork.RestaurantService.GetRestaurantDetail(restaurantId);
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


        [HttpGet("GetRestaurantReviews")]
        public async Task<ResponseModel<ListReviewInfoModel>> GetRestaurantReviews([FromQuery] Guid restaurantId)
        {
            var response = new ResponseModel<ListReviewInfoModel>();
            try
            {
                var payload = await _logicUnitOfWork.RestaurantService.GetAllReviews(restaurantId);
                if (payload != null)
                {
                    response = new ResponseModel<ListReviewInfoModel>
                    {
                        Data = payload,
                        Message = "success",
                        Status = 200
                    };
                }
            }
            catch (ArgumentException ae)
            {
                response = new ResponseModel<ListReviewInfoModel>
                {
                    Data = null,
                    Message = ae.Message,
                    Status = 400
                };
            }
            catch (Exception e)
            {
                response = new ResponseModel<ListReviewInfoModel>
                {
                    Data = null,
                    Message = e.Message,
                    Status = 500
                };
            }
            return response;
        }

        [HttpPut("UpdateReplyReviewInfo")]
        public async Task<ResponseModel<bool>> UpdateReplyReviewInfo([FromBody] UpdateReplyReviewInfoRequest request)
        {
            var response = new ResponseModel<bool>();
            try
            {
                var updateSuccess = await _logicUnitOfWork.RestaurantService.UpdateReplyReviewInfo(request);
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

        [HttpPut("UpdateRestaurantDetail")]
        public async Task<ResponseModel<bool>> UpdateRestaurantDetail([FromForm] RestaurantUpdateModel request)
        {
            var response = new ResponseModel<bool>();
            try
            {
                var updateSuccess = await _logicUnitOfWork.RestaurantService.UpdateRestuarantDatail(request);
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

    }
}
