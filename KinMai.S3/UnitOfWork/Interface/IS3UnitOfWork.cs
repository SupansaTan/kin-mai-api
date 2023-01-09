using KinMai.S3.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinMai.S3.UnitOfWork.Interface
{
    public interface IS3UnitOfWork
    {
        IS3FileService S3FileService { get; set; }
    }
}
