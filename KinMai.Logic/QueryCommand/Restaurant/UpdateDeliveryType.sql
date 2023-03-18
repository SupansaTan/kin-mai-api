
UPDATE "Restaurant"
SET "DeliveryType" = array_append("DeliveryType",'_addType')
WHERE "Id" = '_restaurantId';