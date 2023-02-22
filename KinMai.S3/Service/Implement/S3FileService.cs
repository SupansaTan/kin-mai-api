using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using KinMai.S3.Models;
using KinMai.S3.Service.Interface;
using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinMai.S3.Service.Implement
{
    public class S3FileService : IS3FileService
    {
        private readonly AmazonS3Client S3Client;

        public S3FileService(AmazonS3Client S3Client)
        {
            this.S3Client = S3Client;
        }

        public async Task<string> UploadImage(UploadImageModel model)
        {
            string EncodedFileName = "";
            TransferUtilityConfig config = new TransferUtilityConfig()
            {
                MinSizeBeforePartUpload = 20000000
            };
            TransferUtility fileTransferUtility = new TransferUtility(this.S3Client, config);

            try
            {
                var stream = model.File;
                var request = new TransferUtilityUploadRequest()
                {
                    ContentType = model.ContentType,
                    InputStream = stream,
                    StorageClass = S3StorageClass.ReducedRedundancy,
                    CannedACL = S3CannedACL.PublicRead,
                    BucketName = model.BucketName,
                    Key = model.FileName
                };
                await fileTransferUtility.UploadAsync(request);
                EncodedFileName = model.FileName;
            }
            catch (AmazonS3Exception e)
            {
                if (e.ErrorCode.Equals("InvalidAccessKeyId") || e.ErrorCode.Equals("InvalidSecurity"))
                {
                    throw new Exception("AWS Credentials is invalid.");
                }
                else
                {
                    throw new Exception("Error: " + e.Message);
                }
            }
            return EncodedFileName;
        }
        public async Task<bool> CreateFolder(string bucketName, string path)
        {
            var findFolderRequest = new ListObjectsV2Request();
            findFolderRequest.BucketName = bucketName;
            findFolderRequest.Prefix = $"{path}/";
            findFolderRequest.MaxKeys = 1;

            // check folder is already exists or not
            ListObjectsV2Response findFolderResponse = await S3Client.ListObjectsV2Async(findFolderRequest);
            if (findFolderResponse.S3Objects.Any())
            {
                // folder alreay exists
                return true;
            }

            // create folder
            PutObjectRequest request = new PutObjectRequest()
            {
                BucketName = bucketName,
                StorageClass = S3StorageClass.Standard,
                ServerSideEncryptionMethod = ServerSideEncryptionMethod.None,
                Key = path,
                ContentBody = string.Empty
            };
            PutObjectResponse response = await S3Client.PutObjectAsync(request);
            return response.HttpStatusCode == HttpStatusCode.OK;
        }
        public async Task<bool> DeleteFile(string bucketName, string fileName)
        {
            DeleteObjectRequest request = new DeleteObjectRequest()
            {
                BucketName = bucketName,
                Key = fileName
            };
            DeleteObjectResponse response = await S3Client.DeleteObjectAsync(request);
            return response.HttpStatusCode == HttpStatusCode.OK;
        }
    }
}
