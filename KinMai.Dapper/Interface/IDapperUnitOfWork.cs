using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinMai.Dapper.Interface
{
    public interface IDapperUnitOfWork
    {
        IDapperRepository KinMaiRepository { get; set; }
    }
}
