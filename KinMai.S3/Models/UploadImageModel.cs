using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinMai.S3.Models
{
    public class UploadImageModel
    {
        [Required]
        public Stream File { get; set; }
        [Required]
        public string ContentType { get; set; }
        [Required]
        public string FileName { get; set; }
        [Required]
        public string BucketName { get; set; }
    }
}
