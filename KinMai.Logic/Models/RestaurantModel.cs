using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinMai.Logic.Models
{
    public class RestaurantDetailInfoModel
    {
        public Guid Id { get; set; }
        public RestaurantInfoModel RestaurantInfo { get; set; }
        public float Rating { get; set; }
        public bool IsFavorite { get; set; }
    }

    public class ReviewInfoModel
    {
        public int TotalReview { get; set; }
        public List<ReviewModel> Reviews { get; set; }
    }

    public class ReviewModel
    {
        public Guid Id { get; set; }
        public IFormFile UserImageFiles { get; set; }
        public string UserName { get; set; }
        public float Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreateTime { get; set; }
        public List<IFormFile> ReviewImageFiles { get; set; }
        public string RecommendMenuName { get; set; }
    }
}
