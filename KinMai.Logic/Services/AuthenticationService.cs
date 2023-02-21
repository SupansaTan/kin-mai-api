using System.Net;
using ImageMagick;
using KinMai.Authentication.Model;
using KinMai.Authentication.UnitOfWork;
using KinMai.Common.Enum;
using KinMai.Dapper.Interface;
using KinMai.EntityFramework.Models;
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
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IEntityUnitOfWork _entityUnitOfWork;
        private readonly IDapperUnitOfWork _dapperUnitOfWork;
        private readonly IAuthenticationUnitOfWork _authenticationUnitOfWork;
        private readonly IS3UnitOfWork _S3UnitOfWork;
        private readonly string QUERY_PATH;

        public AuthenticationService(
            IEntityUnitOfWork entityUnitOfWork,
            IAuthenticationUnitOfWork authenticationUnitOfWork,
            IDapperUnitOfWork dapperUnitOfWork,
            IS3UnitOfWork s3UnitOfWork
        )
        {
            QUERY_PATH = this.GetType().Name.Split("Service")[0] + "/";
            _entityUnitOfWork = entityUnitOfWork;
            _authenticationUnitOfWork = authenticationUnitOfWork;
            _dapperUnitOfWork = dapperUnitOfWork;
            _S3UnitOfWork = s3UnitOfWork;
        }
        public async Task<TokenResponseModel> Login(string email, string password)
        {
            // validate user
            var user = await _entityUnitOfWork.UserRepository.GetSingleAsync(x => x.Email.ToLower() == email.ToLower());
            if (user == null)
                throw new ArgumentException("Email does not exist.");
            if (user.IsLoginWithGoogle)
                throw new ArgumentException("This email is registered by Google provider, Please login by Google instead");

            // validate auth
            var access = await _authenticationUnitOfWork.AWSCognitoService.Login(user.Id, password);
            if (access.HttpStatusCode != HttpStatusCode.OK)
                throw new ArgumentException("Invalid Email or password.");

            return new TokenResponseModel
            {
                Token = access.AuthenticationResult.AccessToken,
                ExpiredToken = (DateTime.UtcNow).AddSeconds(access.AuthenticationResult.ExpiresIn),
                RefreshToken = access.AuthenticationResult.RefreshToken
            };
        }
        public async Task<bool> ReviewerRegister(ReviewerRegisterModel model)
        {
            // validate
            var user = await _entityUnitOfWork.UserRepository.GetSingleAsync(x => x.Email.ToLower() == model.Email.ToLower());
            if (user != null) throw new ArgumentException("Email already exists.");
            if (model.Password != model.ConfirmPassword) throw new ArgumentException("Password and Confirm password are not matching");

            // create user
            user = new User
            {
                Id = Guid.NewGuid(),
                Email = model.Email.ToLower(),
                CreateAt = DateTime.UtcNow,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Username = model.Username,
                UserType = (int)UserType.Reviewer,
                IsLoginWithGoogle = !string.IsNullOrEmpty(model.Password)
            };

            if (!(string.IsNullOrEmpty(model.Password) && string.IsNullOrEmpty(model.ConfirmPassword)))
            {
                var singup = await _authenticationUnitOfWork.AWSCognitoService.SignUp(user.Id, user.Email, model.Password);
                if (singup.HttpStatusCode != HttpStatusCode.OK)
                    throw new ArgumentException("Can't register, Please contact admin.");

                var confirmSignup = await _authenticationUnitOfWork.AWSCognitoService.ConfirmSignUp(user.Id);
                if (!confirmSignup)
                    throw new ArgumentException("Can't confirmed register, Please try again.");
            }

            _entityUnitOfWork.UserRepository.Add(user);
            await _entityUnitOfWork.SaveAsync();
            return true;
        }
        public async Task<bool> RestaurantRegister(RestaurantRegisterModel model)
        {
            var user = await CreateUser(model.PersonalInfo, UserType.RestaurantOwner);
            return await RestaurantRegister(model.RestaurantInfo, model.RestaurantAdditionInfo, user);
        }
        public async Task<UserInfoModel> GetUserInfo(string email)
        {
            var user = await _entityUnitOfWork.UserRepository.GetSingleAsync(x => x.Email.ToLower() == email.ToLower());
            if (user == null)
            {
                throw new ArgumentException("Email does not exist.");
            }

            var restaurantName = "";
            var restaurantId = new Guid();
            if (user.UserType == (int)UserType.RestaurantOwner)
            {
                var restaurant = await _entityUnitOfWork.RestaurantRepository.GetSingleAsync(x => x.OwnerId == user.Id);
                restaurantName = restaurant.Name;
                restaurantId = restaurant.Id;
            }
                    
            var userInfo = new UserInfoModel()
            {
                UserId = user.Id,
                UserName = user.Username,
                RestaurantName = restaurantName,
                RestaurantId = restaurantId,
                UserType = (UserType)user.UserType
            };
            return userInfo;
        }
        public async Task<bool> CheckIsLoginWithGoogleFirstTimes(string email)
        {
            var user = await _entityUnitOfWork.UserRepository.GetSingleAsync(x => x.Email == email);
            return user == null;
        }
        private async Task<User> CreateUser(ReviewerRegisterModel model, UserType userType)
        {
            // validate
            var user = await _entityUnitOfWork.UserRepository.GetSingleAsync(x => x.Email.ToLower() == model.Email.ToLower());
            if (user != null) throw new ArgumentException("Email already exists.");
            if (model.Password != model.ConfirmPassword) throw new ArgumentException("Password and Confirm password are not matching");

            // create user
            user = new User
            {
                Id = Guid.NewGuid(),
                Email = model.Email.ToLower(),
                CreateAt = DateTime.UtcNow,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Username = model.Username,
                UserType = (int)userType,
                IsLoginWithGoogle = !string.IsNullOrEmpty(model.Password)
            };

            if (!(string.IsNullOrEmpty(model.Password) && string.IsNullOrEmpty(model.ConfirmPassword)))
            {
                var singup = await _authenticationUnitOfWork.AWSCognitoService.SignUp(user.Id, user.Email, model.Password);
                if (singup.HttpStatusCode != HttpStatusCode.OK)
                    throw new ArgumentException("Can't register, Please contact admin.");

                var confirmSignup = await _authenticationUnitOfWork.AWSCognitoService.ConfirmSignUp(user.Id);
                if (!confirmSignup)
                    throw new ArgumentException("Can't confirmed register, Please try again.");
            }
            return user;
        }
        private async Task<bool> RestaurantRegister(RestaurantInfoModel restaurantInfo, RestaurantPhotoModel additionInfo, User userInfo)
        {
            List<BusinessHour> businessHourList = new List<BusinessHour>();
            List<SocialContact> socialContactList = new List<SocialContact>();
            List<Related> categoryRelatedList = new List<Related>();
            Guid restaurantId = Guid.NewGuid();

            // add all items to list
            foreach (var timeItem in restaurantInfo.BusinessHours)
            {
                BusinessHour item = new BusinessHour()
                {
                    Id = Guid.NewGuid(),
                    RestaurantId = restaurantId,
                    Day = timeItem.Day,
                    OpenTime = TimeOnly.FromDateTime(timeItem.StartTime).AddHours(7),
                    CloseTime = TimeOnly.FromDateTime(timeItem.EndTime).AddHours(7),
                };
                businessHourList.Add(item);
            }

            foreach (var contact in restaurantInfo.Contact)
            {
                SocialContact item = new SocialContact()
                {
                    Id = Guid.NewGuid(),
                    RestaurantId = restaurantId,
                    SocialType = (int)contact.Social,
                    ContactValue = contact.ContactValue,
                };
                socialContactList.Add(item);
            }

            foreach (var category in restaurantInfo.Categories)
            {
                var categoryItem = await _entityUnitOfWork.CategoryRepository.GetSingleAsync(x => x.Type == (int)category);
                Related item = new Related()
                {
                    Id = Guid.NewGuid(),
                    RestaurantId = restaurantId,
                    CategoriesId = categoryItem.Id
                };
                categoryRelatedList.Add(item);
            }

            Restaurant restaurant = new Restaurant()
            {
                Id = restaurantId,
                OwnerId = userInfo.Id,
                Name = restaurantInfo.RestaurantName,
                Description = additionInfo.RestaurantStatus,
                Address = restaurantInfo.Address.Address,
                Latitude = restaurantInfo.Address.Latitude,
                Longitude = restaurantInfo.Address.Longitude,
                MinPriceRate = restaurantInfo.minPriceRate,
                MaxPriceRate = restaurantInfo.maxPriceRate,
                CreateAt = DateTime.UtcNow,
                DeliveryType = restaurantInfo.DeliveryType.ToArray(),
                PaymentMethod = restaurantInfo.PaymentMethods.ToArray(),
                RestaurantType = (int)restaurantInfo.RestaurantType,
            };

            // upload image
            var images = await CompressImage(additionInfo.ImageFiles, restaurantId);
            restaurant.ImageLink = images.ToArray();

            _entityUnitOfWork.UserRepository.Add(userInfo);
            _entityUnitOfWork.RestaurantRepository.Add(restaurant);
            _entityUnitOfWork.BusinessHourRepository.AddRange(businessHourList);
            _entityUnitOfWork.SocialContactRepository.AddRange(socialContactList);
            _entityUnitOfWork.RelatedRepository.AddRange(categoryRelatedList);
            await _entityUnitOfWork.SaveAsync();
            return true;
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

