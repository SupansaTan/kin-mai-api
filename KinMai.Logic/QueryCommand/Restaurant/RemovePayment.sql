
UPDATE "Restaurant"
SET "PaymentMethod" = array_remove("PaymentMethod", '_removeMethod')
WHERE "Id" = '_restaurantId';