namespace KinMai.Logic.Models
{
    public class GetReviewInfoModel
    {
        public string Username { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set;}
        public double CreatedDateDiff { get; set; }
        public List<string> ImageReviewList { get; set; }
        public List<string> FoodRecommendList { get; set; }
        public List<int> ReviewLabelList { get; set; }
        public string RestaurantReply { get; set; }
    }

    public class GetReviewInfoListModel
    {
        public List<GetReviewInfoModel>? ReviewList { get; set; }
        public int TotalReview { get; set; }
        public int TotalReviewHaveImage { get; set; }
        public int TotalReviewHaveComment { get; set; }
        public int TotalReviewHaveFoodRecommend { get; set; }
    }

    public class GetTotalReviewModel
    {
        public int TotalReview { get; set; }
        public int TotalReviewHaveImage { get; set; }
        public int TotalReviewHaveComment { get; set; }
        public int TotalReviewHaveFoodRecommend { get; set; }
    }

    public class GetReviewInfoFilterModel
    {
        public Guid RestaurantId { get; set; }
        public string? Keywords { get; set;}
        public int Rating { get; set; }
        public bool IsOnlyReviewHaveImage { get; set; }
        public bool IsOnlyReviewHaveComment { get; set; }
        public bool IsOnlyReviewHaveFoodRecommend { get; set; }
    }
}
