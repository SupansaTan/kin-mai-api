using System.Net;
using System.Numerics;
using ImageMagick;
using KinMai.Common.Enum;
using KinMai.Common.ShareService;
using KinMai.Dapper.Interface;
using KinMai.EntityFramework.Models;
using KinMai.EntityFramework.UnitOfWork.Implement;
using KinMai.EntityFramework.UnitOfWork.Interface;
using KinMai.Logic.Interface;
using KinMai.Logic.Models;
using KinMai.S3.Models;
using KinMai.S3.UnitOfWork.Interface;
using Microsoft.AspNetCore.Http;
using MimeKit;
using Newtonsoft.Json;

namespace KinMai.Logic.Services
{
    public class RestaurantService : IRestaurantService
    {
        private readonly IEntityUnitOfWork _entityUnitOfWork;
        private readonly IDapperUnitOfWork _dapperUnitOfWork;
        private readonly IS3UnitOfWork _S3UnitOfWork;
        private readonly string QUERY_PATH;

        public RestaurantService(
            IEntityUnitOfWork entityUnitOfWork,
            IDapperUnitOfWork dapperUnitOfWork,
            IS3UnitOfWork s3UnitOfWork
            )
        {
            QUERY_PATH = this.GetType().Name.Split("Service")[0] + "/";
            _entityUnitOfWork = entityUnitOfWork;
            _dapperUnitOfWork = dapperUnitOfWork;
            _S3UnitOfWork = s3UnitOfWork;
        }

        public async Task<RestaurantDetailModel> GetRestaurantDetail(Guid restuarantId)
        {
            var resInfo = await _entityUnitOfWork.RestaurantRepository.GetSingleAsync(x => x.Id == restuarantId);
            if (resInfo != null)
            {
                var query = QueryService.GetCommand(QUERY_PATH + "GetRestaurantArrayData",
                            new ParamCommand { Key = "_restaurantId", Value = restuarantId.ToString() }
                        );
                var data = (await _dapperUnitOfWork.KinMaiRepository.QueryAsync<ResArrayDataModel>(query)).ToList();
                var arrayData = (ResArrayDataModel)data[0];
                var socialContact = _entityUnitOfWork.SocialContactRepository.GetAll(x => x.RestaurantId == restuarantId)
                    .Select(x => new SocialContactModel()
                    {
                        SocialType = x.SocialType,
                        ContactValue = x.ContactValue
                    }).ToList();
                var allCategories = _entityUnitOfWork.CategoryRepository.GetAll();
                var categories = _entityUnitOfWork.RelatedRepository.GetAll(x => x.RestaurantId == restuarantId)
                    .Select(x => new CategoryModel()
                    {
                        CategoryType = allCategories.FirstOrDefault(n => n.Id == x.CategoriesId).Type,
                        CategoryName = allCategories.FirstOrDefault(n => n.Id == x.CategoriesId).Name
                    }).ToList();
                var buHour = _entityUnitOfWork.BusinessHourRepository.GetAll(x => x.RestaurantId == restuarantId)
                    .Select(x => new ResBusinessHourModel()
                    {
                        Day = x.Day,
                        OpenTime = x.OpenTime,
                        CloseTime = x.CloseTime
                    }).ToList();
                return new RestaurantDetailModel()
                {
                    RestaurantInfo = new Restaurant()
                    {
                        Id = resInfo.Id,
                        OwnerId = resInfo.OwnerId,
                        Name = resInfo.Name,
                        Description = resInfo.Description,
                        Address = JsonConvert.SerializeObject(resInfo.Address),
                        CreateAt = resInfo.CreateAt,
                        RestaurantType = (int)resInfo.RestaurantType,
                        Owner = resInfo.Owner,
                        Latitude = resInfo.Latitude,
                        Longitude = resInfo.Longitude,
                        MinPriceRate = resInfo.MinPriceRate,
                        MaxPriceRate = resInfo.MaxPriceRate,
                        ImageLink = arrayData.ImageLink,
                        DeliveryType = arrayData.DeliveryType?.ToArray(),
                        PaymentMethod = arrayData.PaymentMethod?.ToArray(),
                    },
                    SocialContact = socialContact,
                    Categories = categories,
                    BusinessHours = buHour,

                };
            }
            else
            {
                throw new ArgumentException("This restaurant does not exists.");
            }
        }

        public async Task<ListReviewInfoModel> GetAllReviews(Guid restuarantId)
        {
            var isExist = _entityUnitOfWork.RestaurantRepository.GetSingle(x => x.Id == restuarantId);
            if (isExist != null)
            {
                var isReviewsExist = _entityUnitOfWork.ReviewRepository.GetAll(x => x.RestaurantId == restuarantId);
                if (isReviewsExist != null)
                {
                    var query = QueryService.GetCommand(QUERY_PATH + "GetReviewList",
                                new ParamCommand { Key = "_restaurantId", Value = restuarantId.ToString() }
                            );
                    var data = ( await _dapperUnitOfWork.KinMaiRepository.QueryAsync<ReviewInfoModel>(query)).ToList();
                    return new ListReviewInfoModel()
                    {
                        reviews = data
                    };
                }
                else
                {
                    throw new Exception("This reviews does not exists.");
                }
                
            }
            else
            {
                throw new ArgumentException("This restaurant does not exists.");
            }
        }

        public async Task<bool> UpdateReplyReviewInfo(UpdateReplyReviewInfoRequest model)
        {
            var review = await _entityUnitOfWork.ReviewRepository.GetSingleAsync(x => x.Id == model.ReviewId);
            if (review != null)
            {
                review.ReplyComment = model.ReplyComment;
                _entityUnitOfWork.ReviewRepository.Update(review);
                await _entityUnitOfWork.SaveAsync();
                return true;
            }
            else
            {
                throw new ArgumentException("This review does not exists.");
            }
        }

        public async Task<bool> UpdateRestuarantDatail(RestaurantUpdateModel model)
        {
            List<BusinessHour> businessHourList = new List<BusinessHour>();
            List<SocialContact> socialContactList = new List<SocialContact>();
            List<Related> categoryRelatedList = new List<Related>();

            var restaurant = await _entityUnitOfWork.RestaurantRepository.GetSingleAsync(x => x.Id == model.RestaurantId);
            if (restaurant != null)
            {
                
                var newData = model.ResUpdateInfo;

                var query = QueryService.GetCommand(QUERY_PATH + "GetRestaurantArrayData",
                            new ParamCommand { Key = "_restaurantId", Value = model.RestaurantId.ToString() }
                        );
                var data = (await _dapperUnitOfWork.KinMaiRepository.QueryAsync<ResArrayDataModel>(query)).ToList();
                var arrayData = (ResArrayDataModel)data[0];

                // set restaurant info
                restaurant.Name = newData.RestaurantName;
                restaurant.Description = model.RestaurantStatus;
                restaurant.Address = newData.Address.Address;
                restaurant.RestaurantType = (int)newData.RestaurantType;
                restaurant.Latitude = newData.Address.Latitude;
                restaurant.Longitude = newData.Address.Longitude;
                restaurant.MinPriceRate = newData.minPriceRate;
                restaurant.MaxPriceRate = newData.maxPriceRate;

                // set Delivery type
                var setDeliveryType = arrayData.DeliveryType.ToHashSet();
                var setNewDeliveryType = newData.DeliveryType.ToHashSet();
                var removeDeliveryType = from item in arrayData.DeliveryType.Except(newData.DeliveryType)
                                         select item;
                var addDeliveryType = from item in newData.DeliveryType.Except(arrayData.DeliveryType)
                                      select item;

                removeDeliveryType.ToList().ForEach(x =>
                {
                    var query = QueryService.GetCommand(QUERY_PATH + "RemoveDeliveryType",
                            new ParamCommand { Key = "_restaurantId", Value = model.RestaurantId.ToString() },
                            new ParamCommand { Key = "_removeType", Value = x.ToString() }
                        );
                    var data = ( _dapperUnitOfWork.KinMaiRepository.QueryAsync<ResArrayDataModel>(query));
                });
                addDeliveryType.ToList().ForEach(x =>
                {
                    var query = QueryService.GetCommand(QUERY_PATH + "UpdateDeliveryType",
                            new ParamCommand { Key = "_restaurantId", Value = model.RestaurantId.ToString() },
                            new ParamCommand { Key = "_addType", Value = x.ToString() }
                        );
                    var data = (_dapperUnitOfWork.KinMaiRepository.QueryAsync<ResArrayDataModel>(query));
                });

                // set Payment Method
                var setPayment = arrayData.PaymentMethod.ToHashSet();
                var setNewPayment = newData.PaymentMethods.ToHashSet();
                var removePayment = from item in arrayData.PaymentMethod.Except(newData.PaymentMethods)
                                         select item;
                var addPayment = from item in newData.PaymentMethods.Except(arrayData.PaymentMethod)
                                      select item;

                removePayment.ToList().ForEach(x =>
                {
                    var query = QueryService.GetCommand(QUERY_PATH + "RemovePayment",
                            new ParamCommand { Key = "_restaurantId", Value = model.RestaurantId.ToString() },
                            new ParamCommand { Key = "_removeMethod", Value = x.ToString() }
                        );
                    var data = (_dapperUnitOfWork.KinMaiRepository.QueryAsync<ResArrayDataModel>(query));
                });
                addPayment.ToList().ForEach(x =>
                {
                    var query = QueryService.GetCommand(QUERY_PATH + "UpdatePayment",
                            new ParamCommand { Key = "_restaurantId", Value = model.RestaurantId.ToString() },
                            new ParamCommand { Key = "_addMethod", Value = x.ToString() }
                        );
                    var data = (_dapperUnitOfWork.KinMaiRepository.QueryAsync<ResArrayDataModel>(query));
                });


                // update BusinessHours
                foreach (var timeItem in newData.BusinessHours)
                {
                    BusinessHour item = new BusinessHour()
                    {
                        Id = Guid.NewGuid(),
                        RestaurantId = restaurant.Id,
                        Day = timeItem.Day,
                        OpenTime = TimeOnly.FromDateTime(timeItem.StartTime).AddHours(7),
                        CloseTime = TimeOnly.FromDateTime(timeItem.EndTime).AddHours(7),
                    };
                    businessHourList.Add(item);
                }
                // update Contact
                if (newData.Contact is not null && newData.Contact.Any())
                {
                    foreach (var contact in newData.Contact)
                    {
                        SocialContact item = new SocialContact()
                        {
                            Id = Guid.NewGuid(),
                            RestaurantId = restaurant.Id,
                            SocialType = (int)contact.Social,
                            ContactValue = contact.ContactValue,
                        };
                        socialContactList.Add(item);
                    }
                }
                // update Categories
                if (newData.Categories is not null && newData.Categories.Any())
                {
                    foreach (var category in newData.Categories)
                    {
                        var categoryItem = await _entityUnitOfWork.CategoryRepository.GetSingleAsync(x => x.Type == (int)category);
                        Related item = new Related()
                        {
                            Id = Guid.NewGuid(),
                            RestaurantId = restaurant.Id,
                            CategoriesId = categoryItem.Id
                        };
                        categoryRelatedList.Add(item);
                    }
                }

                removePayment.ToList().ForEach(async (x) =>
                {
                    var query = QueryService.GetCommand(QUERY_PATH + "RemovePayment",
                            new ParamCommand { Key = "_restaurantId", Value = model.RestaurantId.ToString() },
                            new ParamCommand { Key = "_removeMethod", Value = x.ToString() }
                        );
                    var data = (await _dapperUnitOfWork.KinMaiRepository.QueryAsync<ResArrayDataModel>(query));
                });
                addPayment.ToList().ForEach(async (x) =>
                {
                    var query = QueryService.GetCommand(QUERY_PATH + "UpdateImage",
                            new ParamCommand { Key = "_restaurantId", Value = model.RestaurantId.ToString() },
                            new ParamCommand { Key = "_addMethod", Value = x.ToString() }
                        );
                    var data = (await _dapperUnitOfWork.KinMaiRepository.QueryAsync<ResArrayDataModel>(query));
                });

                // remove image of old review
                if (model.RemoveImageLink != null && model.RemoveImageLink.Any())
                {
                    //var imageLink = restaurant.ImageLink?.ToList();
                    model.RemoveImageLink.ForEach(async (x) =>
                    {
                        await _S3UnitOfWork.S3FileService.DeleteFile("kinmai", x);
                        var query = QueryService.GetCommand(QUERY_PATH + "RemoveImage",
                            new ParamCommand { Key = "_restaurantId", Value = model.RestaurantId.ToString() },
                            new ParamCommand { Key = "_removeImage", Value = x.ToString() }
                        );
                        var data = (await _dapperUnitOfWork.KinMaiRepository.QueryAsync<ResArrayDataModel>(query));
                    });
                }

                // add new image
                if (model.NewImageFile != null && model.NewImageFile.Any())
                {
                    var images = await CompressImage(model.NewImageFile, restaurant.Id);
                    images.ForEach(async (x) =>
                    {
                        var query = QueryService.GetCommand(QUERY_PATH + "UpdateImage",
                            new ParamCommand { Key = "_restaurantId", Value = model.RestaurantId.ToString() },
                            new ParamCommand { Key = "_addImage", Value = x.ToString() }
                        );
                        var data = (await _dapperUnitOfWork.KinMaiRepository.QueryAsync<ResArrayDataModel>(query));
                    });
                }
                
                // remove old data and add new data
                var oldBuHour = _entityUnitOfWork.BusinessHourRepository.GetAll(x => x.RestaurantId == model.RestaurantId).ToList();
                _entityUnitOfWork.BusinessHourRepository.Delete(oldBuHour);

                var oldContact = _entityUnitOfWork.SocialContactRepository.GetAll(x => x.RestaurantId == model.RestaurantId).ToList();
                _entityUnitOfWork.SocialContactRepository.Delete(oldContact);

                var oldCategory = _entityUnitOfWork.RelatedRepository.GetAll(x => x.RestaurantId == model.RestaurantId).ToList();
                _entityUnitOfWork.RelatedRepository.Delete(oldCategory);
                
                _entityUnitOfWork.RestaurantRepository.Update(restaurant);
                
                _entityUnitOfWork.BusinessHourRepository.AddRange(businessHourList);
                _entityUnitOfWork.SocialContactRepository.AddRange(socialContactList);
                _entityUnitOfWork.RelatedRepository.AddRange(categoryRelatedList);
                
                await _entityUnitOfWork.SaveAsync();
                return true;
            }
            else
            {
                throw new ArgumentException("This restaurant does not exists.");
            }

        }

        private async Task<List<string>> CompressImage(List<IFormFile> files, Guid restaurantId)
        {
            List<string> uploadImageList = new List<string>();

            foreach (var file in files)
            {
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    stream.Position = 0;
                    using (MagickImage image = new MagickImage(stream))
                    {
                        image.Format = image.Format;
                        image.Quality = 10;
                        using (var newImagestream = new MemoryStream())
                        {
                            image.Write(newImagestream);
                            newImagestream.Position = 0;

                            UploadImageResponse currentImage = new UploadImageResponse();
                            string fileName = $"{restaurantId}/{Guid.NewGuid()}";
                            var imageInfo = new UploadImageModel()
                            {
                                ContentType = MimeTypes.GetMimeType(file.FileName),
                                File = newImagestream,
                                FileName = fileName,
                                BucketName = "kinmai"
                            };
                            currentImage.FileName = await _S3UnitOfWork.S3FileService.UploadImage(imageInfo);
                            uploadImageList.Add(currentImage.FileName);
                        }
                    }
                }
            }
            return uploadImageList;
        }
    }
}
