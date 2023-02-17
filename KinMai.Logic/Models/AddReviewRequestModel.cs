using Microsoft.AspNetCore.Http;

namespace KinMai.Logic.Models
{
    public class AddReviewRequestModel
    {
        public Guid UserId { get; set; }
        public Guid RestaurantId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public List<int>? ReviewLabelList { get; set; }
        public List<string>? FoodRecommendList { get; set; }
        public List<IFormFile>? ImageFiles { get; set; }
    }
}