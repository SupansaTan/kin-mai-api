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
    IF exists(SELECT * FROM "Review" WHERE "RestaurantId" = restaurantId)
    THEN
        SELECT SUM(
               CASE
                   WHEN FLOOR("Rating") = 1 THEN "Rating"
                   WHEN FLOOR("Rating") = 2 THEN "Rating" *2
                   WHEN FLOOR("Rating") = 3 THEN "Rating" *3
                   WHEN FLOOR("Rating") = 4 THEN "Rating" *4
                   WHEN FLOOR("Rating") = 5 THEN "Rating" *5
               END
               ) / SUM("Rating") into avg_rating
        FROM "Review"
        WHERE "RestaurantId" = restaurantId;
    END IF;
    RETURN avg_rating;
END;
$rating$ LANGUAGE plpgsql;

SELECT
	"Restaurant"."Id" AS "RestaurantId",
	"Restaurant"."Name" AS "RestaurantName",
	"Restaurant"."ImageLink"[1] AS "ImageCover",
    "Restaurant"."MinPriceRate" AS "MinPriceRate",
	"Restaurant"."MaxPriceRate" AS "MaxPriceRate",
    "Restaurant"."Description" AS "Description",
    (SELECT COUNT(*) FROM "Review" WHERE "Review"."RestaurantId" = "Restaurant"."Id") AS "TotalReview",
    CASE
    	WHEN (now()::time BETWEEN "BusinessHours"."OpenTime" AND "BusinessHours"."CloseTime")
    	THEN TRUE
    	ELSE FALSE
    END AS "IsOpen",
    calculate_rating("Restaurant"."Id") AS "Rating",
	calculate_distance("Restaurant"."Latitude", "Restaurant"."Longitude", '_latitude', '_longitude') AS "Distance"
FROM "FavoriteRestaurant"
LEFT JOIN "Restaurant" ON "Restaurant"."Id" = "FavoriteRestaurant"."RestaurantId"
LEFT JOIN "BusinessHours" ON "BusinessHours"."RestaurantId" = "Restaurant"."Id"
WHERE "FavoriteRestaurant"."UserId" = '_userId' AND "FavoriteRestaurant"."RestaurantId" = "Restaurant"."Id"
ORDER BY "FavoriteRestaurant"."CreateAt" DESC;