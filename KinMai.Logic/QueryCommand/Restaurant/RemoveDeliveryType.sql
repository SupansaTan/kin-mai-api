
UPDATE "Restaurant"
SET "DeliveryType" = array_remove("DeliveryType", '_removeType')
WHERE "Id" = '_restaurantId';