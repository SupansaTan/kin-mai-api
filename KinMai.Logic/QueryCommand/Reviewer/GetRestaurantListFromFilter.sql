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

CREATE OR REPLACE FUNCTION calculate_rating(restaurantId uuid)
RETURNS float AS $rating$
	DECLARE
		avg_rating float = 0;
    BEGIN
	    IF exists(SELECT * FROM "Reviewer" WHERE "RestaurantId" = restaurantId)
	    THEN
	        SELECT SUM(
	            CASE 
	                WHEN FLOOR("CountStar") = 1 THEN "CountStar"
	                WHEN FLOOR("CountStar") = 2 THEN "CountStar" *2
	                WHEN FLOOR("CountStar") = 3 THEN "CountStar" *3
	                WHEN FLOOR("CountStar") = 4 THEN "CountStar" *4
	                WHEN FLOOR("CountStar") = 5 THEN "CountStar" *5        
	            END    
	            ) / SUM("CountStar") into avg_rating
	        FROM "Reviewer"
	        WHERE "RestaurantId" = restaurantId;
	    END IF;
       	RETURN avg_rating;
    END;
$rating$ LANGUAGE plpgsql;

SELECT DISTINCT
	"Restaurant"."Id" AS "RestaurantId",
	"Restaurant"."Name" AS "RestaurantName",
	"Restaurant"."ImageLink"[1] AS "ImageCover",
    "Restaurant"."MinPriceRate" as "MinPriceRate",
	"Restaurant"."MaxPriceRate" as "MaxPriceRate",
    "Restaurant"."Description" as "Description",
	to_char("BusinessHours"."OpenTime", 'HH:mm') AS "StartTime",
	to_char("BusinessHours"."CloseTime", 'HH:mm') AS "EndTime",
    (SELECT COUNT(*) FROM "Reviewer" WHERE "Reviewer"."RestaurantId" = "Id") AS "TotalReview",
    exists(SELECT * from "FavoriteRestaurant" fr WHERE fr."UserId" = '_userId' and fr."RestaurantId" = "Restaurant"."Id") AS "IsFavorite",
    exists(SELECT * from "Reviewer" rv WHERE rv."UserId" = '_userId' and rv."RestaurantId" = "Restaurant"."Id") AS "IsReview",
    calculate_rating("Restaurant"."Id") AS "Rating",
	calculate_distance("Restaurant"."Latitude", "Restaurant"."Longitude", '_latitude', '_longitude') AS "Distance"
FROM "Restaurant"
LEFT JOIN "BusinessHours" ON "BusinessHours"."RestaurantId" = "Restaurant"."Id"
LEFT JOIN "Related" ON "Related"."RestaurantId" = "Restaurant"."Id"
LEFT JOIN "Categories" ON "Categories"."Id" = "Related"."CategoriesId"
WHERE
    -- filter restaurant name
	LOWER("Restaurant"."Name") LIKE ALL(string_to_array('_keywords', ' '))
	AND
	
    -- filter by only open now
    ('_isOpen' = 0 OR 
    (now()::time between "BusinessHours"."OpenTime" and "BusinessHours"."CloseTime"))	
	AND
	
    -- filter by category type
    ('_category' = '{}' OR
	("Categories"."Type" = ANY('_category'::int[])))
    AND

    -- filter by delivery type
    ('_deliveryType' = '{}' OR
    ("Restaurant"."DeliveryType" @> '_deliveryType'::int[]))
	AND 
	
    -- filter by payment method
    ('_paymentMethod' = '{}' OR
	("Restaurant"."PaymentMethod" @> '_paymentMethod'::int[]))
ORDER BY "Rating" DESC, "Distance"
OFFSET '_skip'
LIMIT '_take';