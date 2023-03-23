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
        Task<RestaurantCardListModel> GetRestaurantListFromFilter(GetRestaurantListFromFilterRequestModel model);
        Task<bool> SetFavoriteRestaurant(SetFavoriteResturantRequestModel model);
        Task<bool> AddReviewRestaurant(AddReviewRequestModel model);
        Task<ReviewInfoModel> GetReviewInfo(GetReviewInfoRequest model);
        Task<bool> UpdateReviewInfo(UpdateReviewInfoRequest model);
        Task<GetRestaurantDetailModel> GetRestaurantDetail(GetRestaurantDetailRequestModel model);
        Task<GetReviewInfoListModel> GetRestaurantReviewList(GetReviewInfoFilterModel model);
        Task<List<GetFavoriteRestaurantList>> GetFavoriteRestaurantList(GetFavoriteRestaurantRequest model);
        Task<bool> DeleteReview(Guid reviewId);
    }
}
