using System.Net;
using KinMai.Common.Enum;
using KinMai.Dapper.Interface;
using KinMai.EntityFramework.Models;
using KinMai.EntityFramework.UnitOfWork.Implement;
using KinMai.EntityFramework.UnitOfWork.Interface;
using KinMai.Logic.Interface;
using KinMai.Logic.Models;
using Newtonsoft.Json;

namespace KinMai.Logic.Services
{
    public class RestaurantService : IRestaurantService
    {
        private readonly IEntityUnitOfWork _entityUnitOfWork;
        private readonly IDapperUnitOfWork _dapperUnitOfWork;
        private readonly string QUERY_PATH;

        public RestaurantService(
            IEntityUnitOfWork entityUnitOfWork,
            IDapperUnitOfWork dapperUnitOfWork
            )
        {
            QUERY_PATH = this.GetType().Name.Split("Service")[0] + "/";
            _entityUnitOfWork = entityUnitOfWork;
            _dapperUnitOfWork = dapperUnitOfWork;
        }

        public List<RestaurantDetailInfoModel> GetAllRestaurant()
        {
            var AllInfo = _entityUnitOfWork.RestaurantRepository.GetAll()
                                                                .Select(x => new RestaurantDetailInfoModel()
                                                                {
                                                                    RestaurantInfo = new Restaurant() {
                                                                            Id = x.Id,
                                                                            OwnerId = x.OwnerId,
                                                                            Name = x.Name,
                                                                            Description = x.Description,
                                                                            Address = JsonConvert.SerializeObject(x.Address),
                                                                            CreateAt = x.CreateAt,
                                                                            DeliveryType = x.DeliveryType.ToArray(),
                                                                            PaymentMethod = x.PaymentMethod.ToArray(),
                                                                            RestaurantType = (int)x.RestaurantType,
                                                                    },
                                                                    IsFavorite = false
                                                                }).ToList();
            return AllInfo;
        }

    }
}
