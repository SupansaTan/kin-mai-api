using KinMai.EntityFramework.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinMai.Logic.Models
{
    public class RestaurantDetailModel
    {
        public Restaurant RestaurantInfo { get; set; }
        public List<SocialContactModel> SocialContact { get; set; }
        public List<CategoryModel> Categories { get; set; }
        public List<ResBusinessHourModel> BusinessHours { get; set; }
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
        public List<IFormFile> NewImageFile { get; set; }
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
        public TimeOnly OpenTime { get; set; }
        public TimeOnly CloseTime { get; set; }
    }

    public class ResArrayDataModel
    {
        public string[]? ImageLink { get; set; }
        public int[]? DeliveryType { get; set; }
        public int[]? PaymentMethod { get; set; }
    }

    public class ListReviewInfoModel
    {
        public List<ReviewInfoModel> reviews { get; set; }
    }
}
