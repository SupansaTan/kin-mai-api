namespace KinMai.Api.Models.Reviewer
{
    public class GetRestaurantNearMeRequestModel
    {
        public Guid userId { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public int skip { get; set; }
        public int take { get; set; }
    }

    public class GetRestaurantListFromFilterRequestModel
    {
        public Guid userId { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public int skip { get; set; }
        public int take { get; set; }
        public string? Keywords { get; set; }
        public bool IsOpen { get; set; }
        public List<int>? CategoryType { get; set; }
        public List<int>? DeliveryType { get; set; }
        public List<int>? PaymentMethod { get; set; }
    }
}
