using KinMai.Dapper.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinMai.Dapper.Implement
{
    public class DapperUnitOfWork : IDapperUnitOfWork
    {
        private readonly string ConnectionString;
        private IDapperRepository IKinMaiRepository;

        public DapperUnitOfWork(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public IDapperRepository KinMaiRepository
        {
            get { return IKinMaiRepository ?? (IKinMaiRepository = new DapperRepository(ConnectionString)); }
            set { IKinMaiRepository = value; }
        }
    }
}
