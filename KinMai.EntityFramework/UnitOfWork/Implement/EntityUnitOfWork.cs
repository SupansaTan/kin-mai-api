using KinMai.EntityFramework.Models;
using KinMai.EntityFramework.UnitOfWork.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinMai.EntityFramework.UnitOfWork.Implement
{
    public class EntityUnitOfWork : IEntityUnitOfWork
    {
        private KinMaiContext _dbContext;

        public EntityUnitOfWork(KinMaiContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> SaveAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }
    }
}
