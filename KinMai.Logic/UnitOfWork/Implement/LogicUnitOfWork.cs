using KinMai.Authentication.UnitOfWork;
using KinMai.Dapper.Interface;
using KinMai.EntityFramework.UnitOfWork.Interface;
using KinMai.Logic.Interface;
using KinMai.Logic.Services;
using KinMai.Logic.UnitOfWork.Interface;
using KinMai.S3.UnitOfWork.Interface;
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
        private IS3UnitOfWork _s3UnitOfWork { get; set; }

        public LogicUnitOfWork(
            IEntityUnitOfWork entityUnitOfWork
            , IDapperUnitOfWork dapperUnitOfWork
            , IAuthenticationUnitOfWork authenticationUnitOfWork
            , IS3UnitOfWork s3UnitOfWork
            )
        {
            _entityUnitOfWork = entityUnitOfWork;
            _dapperUnitOfWork = dapperUnitOfWork;
            _authenticationUnitOfWork = authenticationUnitOfWork;
            _s3UnitOfWork = s3UnitOfWork;
        }

        // instance
        private IAuthenticationService _authenticationService;
        private IReviewerService _reviewerService;
        private IFileService _fileService;
        private IRestaurantService _restaurantService;

        public IAuthenticationService AuthenticationService
        {
            get { return _authenticationService ?? (_authenticationService = new AuthenticationService(_entityUnitOfWork, _authenticationUnitOfWork, _dapperUnitOfWork, _s3UnitOfWork)); }

            set { _authenticationService = value; }
        }
        public IReviewerService ReviewerService
        {
            get { return _reviewerService ?? (_reviewerService = new ReviewerService(_entityUnitOfWork, _dapperUnitOfWork, _s3UnitOfWork)); }

            set { _reviewerService = value; }
        }

        public IFileService FileService
        {
            get { return _fileService ?? (_fileService = new FileService(_s3UnitOfWork)); }

            set { _fileService = value; }
        }

        public IRestaurantService RestaurantService
        {
            get { return _restaurantService ?? (_restaurantService = new RestaurantService(_entityUnitOfWork,  _dapperUnitOfWork)); }

            set { _restaurantService = value; }
        }
    }
}
