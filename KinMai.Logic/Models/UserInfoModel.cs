using KinMai.Common.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinMai.Logic.Models
{
    public class UserInfoModel
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public Guid RestaurantId { get; set; }
        public string RestaurantName { get; set; }
        public UserType UserType { get; set; }
    }
}
