using KinMai.Logic.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinMai.Logic.UnitOfWork.Interface
{
    public interface ILogicUnitOfWork
    {
        IAuthenticationService AuthenticationService { get; set; }
        IFileService FileService { get; set; }
        IRestaurantService RestaurantService { get; set; }
    }
}
