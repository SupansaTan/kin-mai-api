
UPDATE "Restaurant"
SET "ImageLink" = array_append("ImageLink",'_addImage')
WHERE "Id" = '_restaurantId';