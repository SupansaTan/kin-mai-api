using System;
namespace KinMai.Logic.Models
{
	public class GetFavoriteRestaurantList
	{
		public Guid RestaurantId { get; set; }
        public string RestaurantName { get; set; }
        public string ImageCover { get; set; }
        public int MinPriceRate { get; set; }
        public int MaxPriceRate { get; set; }
        public string Description { get; set; }
        public int TotalReview { get; set; }
        public bool IsOpen { get; set; }
        public double Rating { get; set; }
        public double Distance { get; set; }
    }

    public class GetFavoriteRestaurantRequest
    {
        public Guid UserId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}

