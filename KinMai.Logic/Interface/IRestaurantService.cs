using KinMai.Logic.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinMai.Logic.Interface
{
    public interface IRestaurantService
    {
        List<RestaurantDetailInfoModel> GetAllRestaurant();
    }
}
