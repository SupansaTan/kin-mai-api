using KinMai.Api.Models.Reviewer;
using KinMai.Common.ShareService;
using KinMai.Dapper.Interface;
using KinMai.EntityFramework.Models;
using KinMai.EntityFramework.UnitOfWork.Interface;
using KinMai.Logic.Interface;
using KinMai.Logic.Models;
using KinMai.S3.UnitOfWork.Interface;
using ImageMagick;
using KinMai.S3.Models;
using Microsoft.AspNetCore.Http;
using MimeKit;
using System.Text;
using Amazon.S3.Model;

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
            var review = await _entityUnitOfWork.ReviewRepository.GetSingleAsync(x => x.UserId == model.UserId && x.RestaurantId == model.RestaurantId);
            if (user is null)
                throw new ArgumentException("This user is not exists.");
            if (restaurant is null)
                throw new ArgumentException("This restaurant is not exists.");
            if (review is not null)
                throw new ArgumentException("You already review this restaurant.");

            var newReview = new Review()
            {
                Id = Guid.NewGuid(),
                UserId = model.UserId,
                RestaurantId = model.RestaurantId,
                Rating = model.Rating,
                Comment = string.IsNullOrEmpty(model.Comment) ? null : model.Comment,
                FoodRecommendList = model.FoodRecommendList?.ToArray() ?? null,
                ReviewLabelRecommend = model.ReviewLabelList?.ToArray() ?? null,
                CreateAt = DateTime.UtcNow
            };

            if (model.ImageFiles is not null && model.ImageFiles.Any())
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
        public async Task<bool> UpdateReviewInfo(UpdateReviewInfoRequest model)
        {
            var review = await _entityUnitOfWork.ReviewRepository.GetSingleAsync(x => x.Id == model.ReviewId);
            if (review != null)
            {
                review.Rating = model.Rating;
                review.Comment = string.IsNullOrEmpty(model.Comment) ? null : model.Comment;
                review.FoodRecommendList = model.FoodRecommendList?.ToArray() ?? null;
                review.ReviewLabelRecommend = model.ReviewLabelList?.ToArray() ?? null;
                
                // remove image of old review
                if (model.RemoveImageLink != null && model.RemoveImageLink.Any())
                {
                    var imageLink = review.ImageLink?.ToList();
                    model.RemoveImageLink.ForEach(async (x) =>
                    {
                        var response = await _S3UnitOfWork.S3FileService.DeleteFile("kinmai", x);
                        imageLink.Remove(x);
                    });
                    review.ImageLink = imageLink.ToArray();
                }

                // add new image
                if (model.NewImageFile != null && model.NewImageFile.Any())
                {
                    var images = await CompressImage(model.NewImageFile, review.UserId, review.RestaurantId);
                    var currentImageLink = review.ImageLink?.ToList() ?? new List<string>();
                    currentImageLink.AddRange(images);
                    review.ImageLink = currentImageLink.ToArray();
                }
                _entityUnitOfWork.ReviewRepository.Update(review);
                await _entityUnitOfWork.SaveAsync();
                return true;
            }
            else
            {
                throw new ArgumentException("This review does not exists.");
            }
        }
        public async Task<GetRestaurantDetailModel> GetRestaurantDetail(GetRestaurantDetailRequestModel model)
        {
            var restaurantIsExist = await _entityUnitOfWork.RestaurantRepository.GetSingleAsync(x => x.Id == model.RestaurantId);
            if (restaurantIsExist is null)
            {
                throw new ArgumentException("Restaurant does not exist.");
            }

            var query = QueryService.GetCommand(QUERY_PATH + "GetRestaurantDetail",
                            new ParamCommand { Key = "_userId", Value = model.UserId.ToString() },
                            new ParamCommand { Key = "_latitude", Value = model.Latitude.ToString() },
                            new ParamCommand { Key = "_longitude", Value = model.Longitude.ToString() },
                            new ParamCommand { Key = "_restaurantId", Value = model.RestaurantId.ToString() }
                        );
            var restaurant = (await _dapperUnitOfWork.KinMaiRepository.QueryAsync<GetRestaurantDetailModel>(query)).ToList();
            return restaurant[0];
        }
        public async Task<GetReviewInfoListModel> GetRestaurantReviewList(GetReviewInfoFilterModel model)
        {
            var restaurantIsExist = await _entityUnitOfWork.RestaurantRepository.GetSingleAsync(x => x.Id == model.RestaurantId);
            if (restaurantIsExist is null)
            {
                throw new ArgumentException("Restaurant does not exist.");
            }

            // get review list
            string keyword = "";
            if (!string.IsNullOrEmpty(model.Keywords))
            {
                var keywords = model.Keywords.Split();
                keywords = keywords.Select(x => x = $"%{x.ToLower()}%").ToArray();
                keyword = string.Join(" ", keywords);
            }
            var query = QueryService.GetCommand(QUERY_PATH + "GetRestaurantReviewList",
                            new ParamCommand { Key = "_rating", Value = model.Rating.ToString() },
                            new ParamCommand { Key = "_keyword", Value = keyword },
                            new ParamCommand { Key = "_isOnlyReviewHaveImage", Value = model.IsOnlyReviewHaveImage? "1":"0" },
                            new ParamCommand { Key = "_isOnlyReviewHaveComment", Value = model.IsOnlyReviewHaveComment ? "1" : "0" },
                            new ParamCommand { Key = "_isOnlyReviewHaveFoodRecommend", Value = model.IsOnlyReviewHaveFoodRecommend ? "1" : "0" },
                            new ParamCommand { Key = "_restaurantId", Value = model.RestaurantId.ToString() }
                        );
            var reviewList = (await _dapperUnitOfWork.KinMaiRepository.QueryAsync<GetReviewInfoModel>(query)).ToList();
            if (reviewList.Any())
            {
                reviewList?.ForEach(x => { x.Username = ReplaceUsername(x.Username); });
            }

            // get total review
            var queryGetTotalReview = QueryService.GetCommand(QUERY_PATH + "GetTotalReview",
                            new ParamCommand { Key = "_restaurantId", Value = model.RestaurantId.ToString() }
                        );
            var totalReview = (await _dapperUnitOfWork.KinMaiRepository.QueryAsync<GetTotalReviewModel>(queryGetTotalReview)).ToList().First();
            
            return new GetReviewInfoListModel()
            {
                ReviewList = reviewList ?? new List<GetReviewInfoModel>(),
                TotalReview = totalReview.TotalReview,
                TotalReviewHaveImage = totalReview.TotalReviewHaveImage,
                TotalReviewHaveComment = totalReview.TotalReviewHaveComment,
                TotalReviewHaveFoodRecommend = totalReview.TotalReviewHaveFoodRecommend,
            };
        }
        private string ReplaceUsername(string username)
        {
            var characters = username[1..(username.Length - 2)].ToCharArray();
            var replace = Array.ConvertAll<char, string>(characters, i => "*");
            return $"{username.First()}{String.Join("", replace)}{username.Last()}";
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
