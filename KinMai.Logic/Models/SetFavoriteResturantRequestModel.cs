using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinMai.Logic.Models
{
    public class SetFavoriteResturantRequestModel
    {
        public Guid UserId { get; set; }
        public Guid RestaurantId { get; set; }
        public bool IsFavorite { get; set; }
    }
}
