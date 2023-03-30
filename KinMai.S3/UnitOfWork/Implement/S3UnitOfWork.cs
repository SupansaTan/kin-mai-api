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
        private IS3FileService IS3FileService;
        private static readonly Lazy<AmazonS3Client> LazyAmazonS3Client = new Lazy<AmazonS3Client>(() => new AmazonS3Client(AWSCredential.AccessKey, AWSCredential.SecretKey, RegionEndpoint.APSoutheast1));
        private static AmazonS3Client Client => LazyAmazonS3Client.Value;

        public S3UnitOfWork() { }

        public IS3FileService S3FileService
        {
            get { return IS3FileService ?? (IS3FileService = new S3FileService(Client)); }
            set { IS3FileService = value; }
        }
    }
}
