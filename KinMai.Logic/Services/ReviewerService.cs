using KinMai.Api.Models.Reviewer;
using KinMai.Common.ShareService;
using KinMai.Dapper.Interface;
using KinMai.EntityFramework.Models;
using KinMai.EntityFramework.UnitOfWork.Interface;
using KinMai.Logic.Interface;
using KinMai.Logic.Models;
using KinMai.S3.UnitOfWork.Interface;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageMagick;
using KinMai.S3.Models;
using Microsoft.AspNetCore.Http;
using MimeKit;

namespace KinMai.Logic.Services
{
    public class ReviewerService : IReviewerService
    {
        private readonly IEntityUnitOfWork _entityUnitOfWork;
        private readonly IDapperUnitOfWork _dapperUnitOfWork;
        private readonly IS3UnitOfWork _S3UnitOfWork;
        private readonly string QUERY_PATH;

        public ReviewerService(
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

        public async Task<RestaurantInfoListModel> GetRestaurantNearMeList(GetRestaurantNearMeRequestModel model)
        {
            var query = QueryService.GetCommand(QUERY_PATH + "GetRestaurantNearMeList",
                            new ParamCommand { Key = "_userId", Value = model.userId.ToString() },
                            new ParamCommand { Key = "_latitude", Value = model.latitude.ToString() },
                            new ParamCommand { Key = "_longitude", Value = model.longitude.ToString() },
                            new ParamCommand { Key = "_skip", Value = model.skip.ToString() },
                            new ParamCommand { Key = "_take", Value = model.take.ToString() }
                        );
            var restaurantInfoList = (await _dapperUnitOfWork.KinMaiRepository.QueryAsync<RestaurantInfoItemModel>(query)).ToList();
            return new RestaurantInfoListModel()
            {
                RestaurantInfo = restaurantInfoList,
                RestaurantCumulativeCount = model.skip + restaurantInfoList.Count,
                TotalRestaurant = _entityUnitOfWork.RestaurantRepository.GetAll().Count()
            };
        }
        public async Task<RestaurantCardListModel> GetRestaurantListFromFilter(GetRestaurantListFromFilterRequestModel model)
        {
            string keyword = "";
            string categoryType = "{}";
            string deliveryType = "{}";
            string paymentMethod = "{}";

            // convert format to support sql command
            if (!string.IsNullOrEmpty(model.Keywords))
            {
                var keywords = model.Keywords.Split();
                keywords = keywords.Select(x => x = $"%{x.ToLower()}%").ToArray();
                keyword = string.Join(" ", keywords);
            }
            if (model.CategoryType != null && model.CategoryType.Any())
            {
                categoryType = "{" + string.Join(",", model.CategoryType) + "}";
            }
            if (model.DeliveryType != null && model.DeliveryType.Any())
            {
                deliveryType = "{" + string.Join(",", model.DeliveryType) + "}";
            }
            if (model.PaymentMethod != null && model.PaymentMethod.Any())
            {
                paymentMethod = "{" + string.Join(",", model.PaymentMethod) + "}";
            }

            var query = QueryService.GetCommand(QUERY_PATH + "GetRestaurantListFromFilter",
                            new ParamCommand { Key = "_userId", Value = model.userId.ToString() },
                            new ParamCommand { Key = "_latitude", Value = model.latitude.ToString() },
                            new ParamCommand { Key = "_longitude", Value = model.longitude.ToString() },
                            new ParamCommand { Key = "_keywords", Value = keyword },
                            new ParamCommand { Key = "_isOpen", Value = model.IsOpen ? "1":"0" },
                            new ParamCommand { Key = "_category", Value = categoryType },
                            new ParamCommand { Key = "_deliveryType", Value = deliveryType },
                            new ParamCommand { Key = "_paymentMethod", Value = paymentMethod }
                        );
            var restaurantInfoList = (await _dapperUnitOfWork.KinMaiRepository.QueryAsync<RestaurantCardInfoModel>(query)).ToList();
            var filterRestaurantList = restaurantInfoList.Skip(model.skip).Take(model.take).ToList();
            return new RestaurantCardListModel()
            {
                RestaurantInfo = filterRestaurantList,
                RestaurantCumulativeCount = model.skip + filterRestaurantList.Count,
                TotalRestaurant = restaurantInfoList.Count
            };
        }
        public async Task<bool> SetFavoriteRestaurant(SetFavoriteResturantRequestModel model)
        {
            var isExist = await _entityUnitOfWork.FavoriteRestaurantRepository.GetSingleAsync(x => x.UserId == model.UserId && x.RestaurantId == model.RestaurantId);
            // favorite restaurant
            if (isExist == null && model.IsFavorite)
            {
                var favoriteItem = new FavoriteRestaurant()
                {
                    Id = Guid.NewGuid(),
                    UserId = model.UserId,
                    RestaurantId = model.RestaurantId,
                };
                _entityUnitOfWork.FavoriteRestaurantRepository.Add(favoriteItem);
            }

            // disfavor restaurant
            if (isExist is not null && !model.IsFavorite)
            {
                _entityUnitOfWork.FavoriteRestaurantRepository.Delete(isExist);
            }

            await _entityUnitOfWork.SaveAsync();
            return true;
        }
        public async Task<bool> AddReviewRestaurant(AddReviewRequestModel model)
        {
            var user = await _entityUnitOfWork.UserRepository.GetSingleAsync(x => x.Id == model.UserId);
            var restaurant =
                await _entityUnitOfWork.RestaurantRepository.GetSingleAsync(x => x.Id == model.RestaurantId);
            if (user is null)
                throw new ArgumentException("This user is not exists.");
            if (restaurant is null)
                throw new ArgumentException("This restaurant is not exists.");

            var newReview = new Review()
            {
                Id = Guid.NewGuid(),
                UserId = model.UserId,
                RestaurantId = model.RestaurantId,
                Rating = model.Rating,
                Comment = model.Comment,
                FoodRecommendList = model.FoodRecommendList.ToArray(),
                ReviewLabelRecommend = model.ReviewLabelList.ToArray(),
                CreateAt = DateTime.UtcNow
            };

            if (model.ImageFiles.Any())
            {
                // upload images
                var images = await CompressImage(model.ImageFiles, model.UserId, model.RestaurantId);
                newReview.ImageLink = images.ToArray();
            }

            _entityUnitOfWork.ReviewRepository.Add(newReview);
            await _entityUnitOfWork.SaveAsync();
            return true;
        }
        public async Task<ReviewInfoModel> GetReviewInfo(GetReviewInfoRequest model)
        {
            var review = await _entityUnitOfWork.ReviewRepository.GetSingleAsync(x => x.UserId == model.UserId && x.RestaurantId == model.RestaurantId);
            if (review != null)
            {
                return new ReviewInfoModel()
                {
                    ReviewId = review.Id,
                    Rating = review.Rating,
                    Comment = review.Comment,
                    ImageLink = review.ImageLink?.ToList() ?? new List<string>(),
                    FoodRecommendList = review.FoodRecommendList?.ToList() ?? new List<string>(),
                    ReviewLabelList = review.ReviewLabelRecommend?.ToList() ?? new List<int>(),
                };
            }
            else
            {
                throw new ArgumentException("This review does not exists.");
            }
        }
        private async Task<List<string>> CompressImage(List<IFormFile> files, Guid userId, Guid restaurantId)
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
                            string fileName = $"{userId}/{restaurantId}/{Guid.NewGuid()}";
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
