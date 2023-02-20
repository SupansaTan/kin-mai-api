select
	(select
		COUNT(*)
	from "Review"
	where "Review"."RestaurantId" = '_restaurantId') as "TotalReview",

	(select
		COUNT(*)
	from "Review"
	where "Review"."RestaurantId" = '_restaurantId'
	and "Review"."ImageLink" is not null) as "TotalReviewHaveImage",

	(select
		COUNT(*)
	from "Review"
	where "Review"."RestaurantId" = '_restaurantId'
	and "Review"."Comment" is not null) as "TotalReviewHaveComment",

	(select
		COUNT(*)
	from "Review"
	where "Review"."RestaurantId" = '_restaurantId'
	and "Review"."FoodRecommendList" is not null) as "TotalReviewHaveFoodRecommend"
from "Review"
where "Review"."RestaurantId" = '_restaurantId';