using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinMai.Logic.Models
{
    public class ReviewInfoModel
    {
        public Guid ReviewId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public List<string> ImageLink { get; set; }
        public List<string> FoodRecommendList { get; set; }
        public List<int> ReviewLabelList { get; set; }
        public DateTime? CreateAt { get; set; }
    }

    public class GetReviewInfoRequest
    {
        public Guid UserId { get; set; }
        public Guid RestaurantId { get; set; }
    }

    public class UpdateReviewInfoRequest
    {
        public Guid ReviewId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public List<string>? RemoveImageLink { get; set; }
        public List<IFormFile>? NewImageFile { get; set; }
        public List<string>? FoodRecommendList { get; set; }
        public List<int>? ReviewLabelList { get; set; }
    }
}
