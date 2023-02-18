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
        public bool IsFavorite { get; set; }
        public List<ReviewInfoModel> Reviews { get; set; }
    }

    public class SocialContactModel
    {
        public int SocialType { get; set; }
        public string ContactValue { get; set; }
    }

}
