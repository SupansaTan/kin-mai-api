select
	"Restaurant"."ImageLink"::text[] as "ImageLink",
	"Restaurant"."DeliveryType"::int[] as "DeliveryType",
	"Restaurant"."PaymentMethod"::int[] as "PaymentMethod"
from "Restaurant"
where "Restaurant"."Id" = '_restaurantId';
