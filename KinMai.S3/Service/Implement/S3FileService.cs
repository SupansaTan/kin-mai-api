using Amazon;
using Amazon.S3;
using KinMai.Authentication.Model;
using KinMai.S3.Models;
using KinMai.S3.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinMai.S3.Service.Implement
{
    public class S3FileService : IS3FileService
    {
        private AmazonS3Client S3Client;

        public S3FileService(AmazonS3Client S3Client)
        {
            this.S3Client = S3Client;
        }

        public async Task<string> UploadImage(UploadImageModel model)
        {
            return "";
        }
    }
}
