using KinMai.Api.Models.Reviewer;
using KinMai.Common.ShareService;
using KinMai.Dapper.Interface;
using KinMai.EntityFramework.UnitOfWork.Interface;
using KinMai.Logic.Interface;
using KinMai.Logic.Models;
using KinMai.S3.UnitOfWork.Interface;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinMai.Logic.Services
{
    public class ReviewerService : IReviewerService
    {
        private readonly IEntityUnitOfWork _entityUnitOfWork;
        private readonly IDapperUnitOfWork _dapperUnitOfWork;
        private readonly IS3UnitOfWork _S3UnitOfWork;
        private readonly string QUERY_PATH;

        public ReviewerService(
            IEntityUnitOfWork entityUnitOfWork,
            IDapperUnitOfWork dapperUnitOfWork,
            IS3UnitOfWork s3UnitOfWork
        )
        {
            QUERY_PATH = this.GetType().Name.Split("Service")[0] + "/";
            _entityUnitOfWork = entityUnitOfWork;
            _dapperUnitOfWork = dapperUnitOfWork;
            _S3UnitOfWork = s3UnitOfWork;
        }

        public async Task<RestaurantInfoListModel> GetRestaurantNearMeList(GetRestaurantNearMeRequestModel model)
        {
            var query = QueryService.GetCommand(QUERY_PATH + "GetRestaurantNearMeList",
                            new ParamCommand { Key = "_userId", Value = model.userId.ToString() },
                            new ParamCommand { Key = "_latitude", Value = model.latitude.ToString() },
                            new ParamCommand { Key = "_longitude", Value = model.longitude.ToString() },
                            new ParamCommand { Key = "_skip", Value = model.skip.ToString() },
                            new ParamCommand { Key = "_take", Value = model.take.ToString() }
                        );
            var restaurantInfoList = (await _dapperUnitOfWork.KinMaiRepository.QueryAsync<RestaurantInfoItemModel>(query)).ToList();
            return new RestaurantInfoListModel()
            {
                RestaurantInfo = restaurantInfoList,
                RestaurantCumulativeCount = model.skip + restaurantInfoList.Count,
                TotalRestaurant = _entityUnitOfWork.RestaurantRepository.GetAll().Count()
            };
        }
    }
}
