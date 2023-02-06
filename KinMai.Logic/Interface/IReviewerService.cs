using KinMai.Api.Models.Reviewer;
using KinMai.Logic.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinMai.Logic.Interface
{
    public interface IReviewerService
    {
        Task<RestaurantInfoListModel> GetRestaurantNearMeList(GetRestaurantNearMeRequestModel model);
        Task<bool> SetFavoriteRestaurant(SetFavoriteResturantRequestModel model);
    }
}
