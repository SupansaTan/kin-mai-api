using System.Net;
using KinMai.Common.Enum;
using KinMai.Dapper.Interface;
using KinMai.EntityFramework.Models;
using KinMai.EntityFramework.UnitOfWork.Implement;
using KinMai.EntityFramework.UnitOfWork.Interface;
using KinMai.Logic.Interface;
using KinMai.Logic.Models;
using Newtonsoft.Json;

namespace KinMai.Logic.Services
{
    public class RestaurantService : IRestaurantService
    {
        private readonly IEntityUnitOfWork _entityUnitOfWork;
        private readonly IDapperUnitOfWork _dapperUnitOfWork;
        private readonly string QUERY_PATH;

        public RestaurantService(
            IEntityUnitOfWork entityUnitOfWork,
            IDapperUnitOfWork dapperUnitOfWork
            )
        {
            QUERY_PATH = this.GetType().Name.Split("Service")[0] + "/";
            _entityUnitOfWork = entityUnitOfWork;
            _dapperUnitOfWork = dapperUnitOfWork;
        }

        public async Task<RestaurantDetailModel> GetRestaurantDetail(GetReviewInfoRequest model)
        {
            var resInfo = await _entityUnitOfWork.RestaurantRepository.GetSingleAsync(x => x.Id == model.RestaurantId);
            if (resInfo != null)
            {
                var reviews = GetAllReviews(model);
                var socialContact = _entityUnitOfWork.SocialContactRepository.GetAll(x => x.RestaurantId == model.RestaurantId)
                    .Select(x => new SocialContactModel()
                    {
                        SocialType = x.SocialType,
                        ContactValue = x.ContactValue
                    }).ToList();
                var isFav = await _entityUnitOfWork.FavoriteRestaurantRepository.GetSingleAsync(x => x.UserId == model.UserId && x.RestaurantId == model.RestaurantId);
                return new RestaurantDetailModel()
                {
                    RestaurantInfo = new Restaurant()
                    {
                        Id = resInfo.Id,
                        OwnerId = resInfo.OwnerId,
                        Name = resInfo.Name,
                        ImageLink = resInfo.ImageLink,
                        Description = resInfo.Description,
                        Address = JsonConvert.SerializeObject(resInfo.Address),
                        CreateAt = resInfo.CreateAt,
                        DeliveryType = resInfo.DeliveryType?.ToArray(),
                        PaymentMethod = resInfo.PaymentMethod?.ToArray(),
                        RestaurantType = (int)resInfo.RestaurantType,
                        Owner = resInfo.Owner,
                        Latitude = resInfo.Latitude,
                        Longitude = resInfo.Longitude,
                        MinPriceRate = resInfo.MinPriceRate,
                        MaxPriceRate = resInfo.MaxPriceRate
                    },
                    SocialContact = socialContact,
                    IsFavorite = (isFav != null),
                    Reviews = reviews
                };
            }
            else
            {
                throw new ArgumentException("This restaurant does not exists.");
            }
        }

        public List<ReviewInfoModel> GetAllReviews (GetReviewInfoRequest model)
        {
            var reviews = _entityUnitOfWork.ReviewRepository.GetAll(x => x.RestaurantId == model.RestaurantId);
            if (reviews != null)
            {
                var myReview = reviews.Select(x => new ReviewInfoModel()
                    {
                        ReviewId = x.Id,
                        Rating = x.Rating,
                        Comment = x.Comment,
                        ImageLink = x.ImageLink.ToList() ?? new List<string>(),
                        FoodRecommendList = x.FoodRecommendList.ToList() ?? new List<string>(),
                        ReviewLabelList = x.ReviewLabelRecommend.ToList() ?? new List<int>(),
                    }).ToList();
                return myReview;
            }
            else
            {
                return new List<ReviewInfoModel>();
            }
        }
    }
}
