using KinMai.Common.Enum;

namespace KinMai.Logic.Models
{
    public class GetRestaurantDetailModel
    {
        public Guid RestaurantId { get; set; }
        public string RestaurantName { get; set; }
        public int MinPriceRate { get; set; }
        public int MaxPriceRate { get; set; }
        public double Distance { get; set; }
        public double Rating { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public int TotalReview { get; set; }
        public bool IsReview { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public List<string> FoodRecommendList { get; set; } 
        public List<string> ImageCover { get; set; }
        public List<int> CategoryList { get; set; }
        public List<int> DeliveryTypeList { get; set; }
        public List<int> PaymentMethodList { get; set; }
        public List<string> SocialContactList { get; set; }
    }

    public class SocialContactItemModel
    {
        public SocialContactType SocialType { get; set; }
        public string ContactValue { get; set; }
    }

    public class GetRestaurantDetailRequestModel
    {
        public Guid RestaurantId { get; set; }
        public Guid? UserId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
