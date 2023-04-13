SELECT
	COUNT(*) AS "TotalReview",

	SUM(CASE
			WHEN "Review"."ImageLink" IS NOT NULL
			THEN 1
		END) AS "TotalReviewHaveImage",

	SUM(CASE
		WHEN "Review"."Comment" IS NOT NULL
		THEN 1
	END) AS "TotalReviewHaveComment",

	SUM(CASE
		WHEN "Review"."FoodRecommendList" IS NOT NULL
		THEN 1
	END) AS "TotalReviewHaveFoodRecommend"
FROM "Review"
WHERE "Review"."RestaurantId" = '_restaurantId'
	and
	
	-- filter comment, food recommend
	LOWER("Review"."Comment") LIKE ALL(string_to_array('_keyword', ' '))
	and
	
	-- filter by rating
    ('_rating' = '6' OR 
    ("Review"."Rating" = '_rating'::int));