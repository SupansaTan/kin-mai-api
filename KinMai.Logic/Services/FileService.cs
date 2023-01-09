using KinMai.Logic.Interface;
using KinMai.Logic.Models;
using KinMai.S3.Models;
using KinMai.S3.UnitOfWork.Interface;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinMai.Logic.Services
{
    public class FileService : IFileService
    {
        private readonly IS3UnitOfWork S3UnitOfWork;

        public FileService(IS3UnitOfWork s3UnitOfWork) 
        {
            this.S3UnitOfWork = s3UnitOfWork;
        }

        public async Task<List<UploadImageResponse>> UploadRestaurantImage(List<IFormFile> files, Guid restaurantId)
        {
            try
            {
                List<UploadImageResponse> uploadImageList = new List<UploadImageResponse>();

                foreach (var file in files)
                {
                    UploadImageResponse image = new UploadImageResponse();
                    string fileName = $"{restaurantId}/{Guid.NewGuid()}";

                    var imageInfo = new UploadImageModel()
                    {
                        ContentType = file.ContentType,
                        File = file.OpenReadStream(),
                        FileName = fileName,
                        BucketName = "kinmai"
                    };
                    image.FileName = await S3UnitOfWork.S3FileService.UploadImage(imageInfo);
                    uploadImageList.Add(image);
                }
                return uploadImageList;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message, e);
            }
        }
    }
}
