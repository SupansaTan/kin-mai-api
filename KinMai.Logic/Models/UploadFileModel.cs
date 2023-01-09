using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinMai.Logic.Models
{
    public class UploadFileModel
    {
        [Required]
        public List<IFormFile> Files { get; set; }
    }

    public class UploadImageResponse
    {
        public string FileName { get; set; }
    }
}
