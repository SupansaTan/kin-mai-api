using KinMai.Common.Enum;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinMai.Logic.Models
{
    public class RestaurantRegisterModel
    {
        public ReviewerRegisterModel PersonalInfo { get; set; }
        public RestaurantInfoModel RestaurantInfo { get; set; }
        public RestaurantPhotoModel RestaurantAdditionInfo { get; set; }
    }

    public class RestaurantInfoModel
    {
        public string RestaurantName { get; set; }
        public int minPriceRate { get; set; }
        public int maxPriceRate { get; set; }
        public RestaurantAddressModel Address { get; set; }
        public RestaurantType RestaurantType { get; set; }
        public List<int> DeliveryType { get; set; }
        public List<RestaurantCategories> Categories { get; set; }
        public List<int> PaymentMethods { get; set; }
        public List<RestaurantContactModel> Contact { get; set; }
        public List<BusinessHourModel> BusinessHours { get; set; }
    }

    public class RestaurantContactModel
    {
        public SocialContactType Social { get; set; }
        public string ContactValue { get; set; }
    }

    public class RestaurantAddressModel
    {
        public string Address { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class BusinessHourModel
    {
        public int Day { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
    }

    public class RestaurantPhotoModel
    {
        public List<IFormFile> ImageFiles { get; set; }
        public string RestaurantStatus { get; set; }
    }
}
