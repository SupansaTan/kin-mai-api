﻿using KinMai.Logic.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinMai.Logic.Interface
{
    public interface IRestaurantService
    {
        Task<RestaurantDetailModel> GetRestaurantDetail(Guid restuarantId);
        List<ReviewInfoModel> GetAllReviews(Guid restaurantId);
        Task<bool> UpdateReplyReviewInfo(UpdateReplyReviewInfoRequest model);
        Task<bool> UpdateRestuarantDatail(RestaurantUpdateModel model);
    }
}
