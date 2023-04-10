using KinMai.EntityFramework.Models;
using Microsoft.AspNetCore.Http;

namespace KinMai.Logic.Models
{
    public class RestaurantDetailModel
    {
        public Restaurant RestaurantInfo { get; set; }
        public List<SocialContactModel> SocialContact { get; set; }
        public List<CategoryModel> Categories { get; set; }
        public List<string> BusinessHours { get; set; }
    }

    public class SocialContactModel
    {
        public int SocialType { get; set; }
        public string ContactValue { get; set; }
    }

    public class RestaurantUpdateModel
    {
        public Guid RestaurantId { get; set; }
        public RestaurantInfoModel ResUpdateInfo { get; set; }
        public List<string>? RemoveImageLink { get; set; }
        public List<IFormFile>? NewImageFile { get; set; }
        public string? RestaurantStatus { get; set; }
    }
    public class CategoryModel
    {
        public int CategoryType { get; set; }
        public string CategoryName { get; set; }
    }
    public class ResBusinessHourModel
    {
        public int Day { get; set; }
        public string OpenTime { get; set; }
        public string CloseTime { get; set; }
    }

    public class ResArrayDataModel
    {
        public string[]? ImageLink { get; set; }
        public int[]? DeliveryType { get; set; }
        public int[]? PaymentMethod { get; set; }
        public List<string> BusinessHour { get; set; }
    }

    public class ListReviewInfoModel
    {
        public List<ReviewInfoModel> reviews { get; set; }
    }
}
