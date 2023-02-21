SELECT
	COUNT(*) AS "TotalReview",

	(SELECT
		COUNT(*)
	FROM "Review"
	WHERE "Review"."RestaurantId" = '_restaurantId'
	AND "Review"."ImageLink" IS NOT NULL) AS "TotalReviewHaveImage",

	(SELECT
		COUNT(*)
	FROM "Review"
	WHERE "Review"."RestaurantId" = '_restaurantId'
	AND "Review"."Comment" IS NOT NULL) AS "TotalReviewHaveComment",

	(SELECT
		COUNT(*)
	FROM "Review"
	WHERE "Review"."RestaurantId" = '_restaurantId'
	AND "Review"."FoodRecommendList" IS NOT NULL) AS "TotalReviewHaveFoodRecommend"
FROM "Review"
WHERE "Review"."RestaurantId" = '_restaurantId';