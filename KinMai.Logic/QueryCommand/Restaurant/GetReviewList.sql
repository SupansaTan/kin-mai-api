select
	"Review"."Id" as "ReviewId",
	"Review"."Rating" as "Rating",
	CASE
		WHEN "Review"."Comment" = 'null'
		THEN ''
		ELSE "Review"."Comment"
	END AS "Comment",
	"Review"."ImageLink"::text[] as "ImageLink",
	"Review"."FoodRecommendList"::text[] as "FoodRecommendList",
	"Review"."ReviewLabelRecommend" as "ReviewLabelList",
	"Review"."CreateAt",
	"User"."Id" as "UserId",
	"User"."Username" as "Username",
	"Review"."ReplyComment"
from "Review"
left join "User" ON "User"."Id" = "Review"."UserId" 
where "Review"."RestaurantId" = '_restaurantId'
order by "Review"."CreateAt";