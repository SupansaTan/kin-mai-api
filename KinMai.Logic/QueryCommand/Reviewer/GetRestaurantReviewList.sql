﻿select
	u."Username" as "Username",
	"Review"."Rating" as "Rating",
	"Review"."Comment" as "Comment",
	(now() - "Review"."CreateAt")::text as "CreatedDateDiff",
	"Review"."ImageLink" as "ImageReviewList",
	"Review"."FoodRecommendList" as "FoodRecommendList",
	"Review"."ReviewLabelRecommend" as "ReviewLabelList"
from "Review"
left join "User" u ON u."Id" = "Review"."UserId" 
where "Review"."RestaurantId" = '_restaurantId'
	and
	
	-- filter comment, food recommend
	LOWER("Review"."Comment") LIKE ALL(string_to_array('_keyword', ' '))
	and
	
	-- filter by get only review that have image
    ('_isOnlyReviewHaveImage' = '0' OR 
    ("Review"."ImageLink" is not null))	
	and
	
	-- filter by get only review that have comment
    ('_isOnlyReviewHaveComment' = '0' OR 
    ("Review"."Comment" is not null))	
	and
	
	-- filter by get only review that have food recommend
    ('_isOnlyReviewHaveFoodRecommend' = '0' OR 
    ("Review"."FoodRecommendList" is not null))
    and 
    
    -- filter by rating
    ('_rating' = '6' OR 
    ("Review"."Rating" = '_rating'::int))
order by (now() - "Review"."CreateAt"), "Rating" DESC;