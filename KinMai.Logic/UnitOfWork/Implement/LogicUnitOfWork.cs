using KinMai.Authentication.UnitOfWork;
using KinMai.Dapper.Interface;
using KinMai.EntityFramework.UnitOfWork.Interface;
using KinMai.Logic.Interface;
using KinMai.Logic.Services;
using KinMai.Logic.UnitOfWork.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinMai.Logic.UnitOfWork.Implement
{
    public class LogicUnitOfWork : ILogicUnitOfWork
    {
        private IEntityUnitOfWork _entityUnitOfWork { get; set; }
        private IDapperUnitOfWork _dapperUnitOfWork { get; set; }
        private IAuthenticationUnitOfWork _authenticationUnitOfWork { get; set; }

        public LogicUnitOfWork(
            IEntityUnitOfWork entityUnitOfWork
            , IDapperUnitOfWork dapperUnitOfWork
            , IAuthenticationUnitOfWork authenticationUnitOfWork
            )
        {
            _entityUnitOfWork = entityUnitOfWork;
            _dapperUnitOfWork = dapperUnitOfWork;
            _authenticationUnitOfWork = authenticationUnitOfWork;
        }

        // instance
        private IAuthenticationService _authenticationService;

        public IAuthenticationService AuthenticationService
        {
            get { return _authenticationService ?? (_authenticationService = new AuthenticationService(_entityUnitOfWork, _authenticationUnitOfWork, _dapperUnitOfWork)); }

            set { _authenticationService = value; }
        }
    }
}
