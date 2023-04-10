CREATE OR REPLACE FUNCTION calculate_distance(lat1 float, lon1 float, lat2 float, lon2 float)
RETURNS float AS $dist$
    DECLARE
        dist float = 0;
        radlat1 float;
        radlat2 float;
        theta float;
        radtheta float;
    BEGIN
        IF lat1 = lat2 OR lon1 = lon2
            THEN RETURN dist;
        ELSE
            radlat1 = pi() * lat1 / 180;
            radlat2 = pi() * lat2 / 180;
            theta = lon1 - lon2;
            radtheta = pi() * theta / 180;
            dist = sin(radlat1) * sin(radlat2) + cos(radlat1) * cos(radlat2) * cos(radtheta);

            IF dist > 1 THEN dist = 1; END IF;

            dist = acos(dist);
            dist = dist * 180 / pi();
            dist = dist * 60 * 1.1515 * 1609.344;
            RETURN dist;
        END IF;
    END;
$dist$ LANGUAGE plpgsql;

create OR REPLACE FUNCTION calculate_rating(restaurantId uuid)
RETURNS float AS $rating$
	declare
		avg_rating float = 0;
    begin
	    if exists(select * FROM "Review" where "RestaurantId" = restaurantId)
	    then
	        SELECT AVG("Rating")::numeric(10,1) into avg_rating
	        FROM "Review"
	        where "RestaurantId" = restaurantId;
	    end if;
       	return avg_rating;
    END;
$rating$ LANGUAGE plpgsql;

select
	"Restaurant"."Id" as "RestaurantId",
	"Restaurant"."Name" as "RestaurantName",
	"Restaurant"."MinPriceRate" as "MinPriceRate",
	"Restaurant"."MaxPriceRate" as "MaxPriceRate",
	calculate_distance("Restaurant"."Latitude", "Restaurant"."Longitude", '_latitude', '_longitude') as "Distance",
	calculate_rating("Restaurant"."Id") as "Rating",
	to_char("BusinessHours"."OpenTime", 'HH24:MI') as "StartTime",
	to_char("BusinessHours"."CloseTime", 'HH24:MI') as "EndTime",
	(SELECT COUNT(*) FROM "Review" WHERE "Review"."RestaurantId" = "Restaurant"."Id") as "TotalReview",
	exists(SELECT * from "Review" rv WHERE rv."UserId" = '_userId' and rv."RestaurantId" = "Restaurant"."Id") AS "IsReview",
	"Restaurant"."Description" as "Description",
	"Restaurant"."Address" as "Address",
	"Restaurant"."Latitude" as "Latitude",
	"Restaurant"."Longitude" as "Longitude",
	"Restaurant"."ImageLink"::text[] as "ImageCover",
	(select 
		array_agg(foodRecommend)::text[]
	from (
		select unnest("FoodRecommendList")
		from "Review"
		where "Review"."RestaurantId" = '_restaurantId'
	) as dt(foodRecommend)) as "FoodRecommendList",
	(select 
		array_agg(category)::int[]
	from (
		select c."Type"
		from "Restaurant"
		left join "Related" r on r."RestaurantId" = "Restaurant"."Id" 
		left join "Categories" c on c."Id" = r."CategoriesId"
		where "Restaurant"."Id" = '_restaurantId'
	) as dt(category)) as "CategoryList",
	"Restaurant"."DeliveryType"::int[] as "DeliveryTypeList",
	"Restaurant"."PaymentMethod"::int[] as "PaymentMethodList",
	(select 
		array (
			select json_build_object('SocialType', sc."SocialType",'ContactValue', sc."ContactValue")
			from "Restaurant"
			left join "SocialContact" sc on sc."RestaurantId" = "Restaurant"."Id" 
			where "Restaurant"."Id" = '_restaurantId'
		)::json[]
	) as "SocialContactList"
from "Restaurant"
left join "BusinessHours" on "BusinessHours"."RestaurantId" = "Restaurant"."Id"
where "Restaurant"."Id" = '_restaurantId';
