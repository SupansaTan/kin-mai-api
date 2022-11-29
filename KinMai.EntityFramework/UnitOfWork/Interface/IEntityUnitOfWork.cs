using KinMai.EntityFramework.Models;
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
        IEntityFrameworkNpgsqlRepository<Related> RelatedRepository { get; set; }
        IEntityFrameworkNpgsqlRepository<User> UserRepository { get; set; }
        IEntityFrameworkNpgsqlRepository<Restaurant> RestaurantRepository { get; set; }
        IEntityFrameworkNpgsqlRepository<BusinessHour> BusinessHourRepository { get; set; }
        IEntityFrameworkNpgsqlRepository<SocialContact> SocialContactRepository { get; set; }
        IEntityFrameworkNpgsqlRepository<Reviewer> ReviewerRepository { get; set; }
        IEntityFrameworkNpgsqlRepository<Category> CategoryRepository { get; set; }
    }
}
