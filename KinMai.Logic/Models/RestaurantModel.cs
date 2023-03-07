using KinMai.EntityFramework.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
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


}
