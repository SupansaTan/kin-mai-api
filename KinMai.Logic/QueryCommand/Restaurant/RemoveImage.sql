
UPDATE "Restaurant"
SET "ImageLink" = array_remove("ImageLink", '_removeImage')
WHERE "Id" = '_restaurantId';