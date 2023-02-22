using KinMai.S3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinMai.S3.Service.Interface
{
    public interface IS3FileService
    {
        Task<string> UploadImage(UploadImageModel model);
        Task<bool> CreateFolder(string bucketName, string path);
        Task<bool> DeleteFile(string bucketName, string fileName);
    }
}
