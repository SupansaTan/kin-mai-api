using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinMai.EntityFramework.UnitOfWork.Interface
{
    public interface IEntityUnitOfWork
    {
        Task<int> SaveAsync();
    }
}
