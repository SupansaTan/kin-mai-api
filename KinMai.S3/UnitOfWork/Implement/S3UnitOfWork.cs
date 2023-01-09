using Amazon;
using Amazon.S3;
using KinMai.Authentication.Model;
using KinMai.S3.Service.Implement;
using KinMai.S3.Service.Interface;
using KinMai.S3.UnitOfWork.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinMai.S3.UnitOfWork.Implement
{
    public class S3UnitOfWork : IS3UnitOfWork
    {
        private readonly AmazonS3Client S3Client;
        private IS3FileService IS3FileService;

        public S3UnitOfWork()
        {
            S3Client = new AmazonS3Client(AWSCredential.AccessKey, AWSCredential.SecretKey, RegionEndpoint.APSoutheast1);
        }

        public IS3FileService S3FileService
        {
            get { return IS3FileService ?? (IS3FileService = new S3FileService(S3Client)); }
            set { IS3FileService = value; }
        }
    }
}
