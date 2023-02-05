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
}
