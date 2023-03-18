
UPDATE "Restaurant"
SET "PaymentMethod" = array_append("PaymentMethod",'_addMethod')
WHERE "Id" = '_restaurantId';