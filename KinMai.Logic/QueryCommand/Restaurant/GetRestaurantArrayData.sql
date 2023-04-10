select
	"Restaurant"."ImageLink"::text[] as "ImageLink",
	"Restaurant"."DeliveryType"::int[] as "DeliveryType",
	"Restaurant"."PaymentMethod"::int[] as "PaymentMethod",
	(select 
		array (
			select json_build_object('Day', "BusinessHours"."Day",'OpenTime', to_char("BusinessHours"."OpenTime", 'HH24:MI'), 'CloseTime', to_char("BusinessHours"."CloseTime", 'HH24:MI'))
			from "BusinessHours"
			where "BusinessHours"."RestaurantId" = '_restaurantId'
		)::json[]
	) as "BusinessHour"
from "Restaurant"
where "Restaurant"."Id" = '_restaurantId';
