using KinMai.EntityFramework.Models;
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
        public Restaurant RestaurantInfo { get; set; }
        public bool IsFavorite { get; set; }
    }

    public class RestaurantReviewsModel
    {
        public int TotalReview { get; set; }
        public List<ReviewModel> Reviews { get; set; }
    }

    public class ReviewModel
    {
        public Reviewer Reviews { get; set; }
        public IFormFile UserImageFiles { get; set; }
        public string UserName { get; set; }
    }
}
