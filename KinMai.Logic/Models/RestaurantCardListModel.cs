using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinMai.Logic.Models
{
    public class RestaurantCardListModel
    { 
        public List<RestaurantCardInfoModel> RestaurantInfo { get; set; }
        public int TotalRestaurant { get; set; }
        public int RestaurantCumulativeCount { get; set; }
    }

    public class RestaurantCardInfoModel
    {
        public Guid RestaurantId { get; set; }
        public string RestaurantName { get; set; }
        public double Rating { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public double Distance { get; set; }
        public int MinPriceRate { get; set; }
        public int MaxPriceRate { get; set; }
        public string ImageCover { get; set; }
        public string Description { get; set; }
        public int TotalReview { get; set; }
        public bool IsFavorite { get; set; }
    }
}
