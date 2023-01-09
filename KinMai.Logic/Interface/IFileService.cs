using KinMai.Logic.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinMai.Logic.Interface
{
    public interface IFileService
    {
        Task<List<UploadImageResponse>> UploadRestaurantImage(List<IFormFile> files, Guid restaurantId);
    }
}
