using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using KinMai.EntityFramework.Models;
using KinMai.EntityFramework.UnitOfWork.Interface;
using Microsoft.EntityFrameworkCore;

namespace KinMai.EntityFramework.UnitOfWork.Implement
{
    public class EntityFrameworkNpgsqlRepository<T> : IEntityFrameworkNpgsqlRepository<T> where T : class
    {
        protected readonly KinMaiContext dbContext;

        public EntityFrameworkNpgsqlRepository(KinMaiContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public Task<int> CountAsync()
        {
            return dbContext.Set<T>().CountAsync();
        }

        public virtual void Delete(T entity)
        {
            dbContext.Set<T>().Remove(entity);
        }

        public virtual void Delete(List<T> entity)
        {
            dbContext.Set<T>().RemoveRange(entity);
        }

        public virtual IQueryable<T> GetAll()
        {
            return dbContext.Set<T>().AsQueryable();
        }

        public virtual IQueryable<T> GetAll(Expression<Func<T, bool>> predicate)
        {
            return dbContext.Set<T>().Where(predicate).AsQueryable();
        }

        public IQueryable<T> GetAll(params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = dbContext.Set<T>();
            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }
            return query.AsQueryable();
        }

        public T GetSingle(Expression<Func<T, bool>> predicate)
        {
            return dbContext.Set<T>().FirstOrDefault(predicate);
        }

        public Task<T> GetSingleAsync(Expression<Func<T, bool>> predicate)
        {
            return dbContext.Set<T>().FirstOrDefaultAsync(predicate);
        }

        public Task<T> GetSingleAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = dbContext.Set<T>();
            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }

            return query.Where(predicate).FirstOrDefaultAsync();
        }

        public async Task<T> AddAsync(T entity)
        {
            await dbContext.Set<T>().AddAsync(entity).ConfigureAwait(false);
            return entity;
        }

        public T Add(T entity)
        {
            dbContext.Set<T>().Add(entity);
            return entity;
        }

        public List<T> AddRange(List<T> entity)
        {
            dbContext.Set<T>().AddRange(entity);
            return entity;
        }

        public async Task<IEnumerable<T>> AddAsync(IEnumerable<T> entity)
        {
            await dbContext.Set<T>().AddRangeAsync(entity).ConfigureAwait(false);
            return entity;
        }

        public void Update(T entity)
        {
            dbContext.Entry(entity).State = EntityState.Modified;
        }

        public IEnumerable<T> UpdateRange(IEnumerable<T> entity)
        {
            dbContext.Set<T>().UpdateRange(entity);
            return entity;
        }

        public virtual bool All(Expression<Func<T, bool>> predicate)
        {
            return dbContext.Set<T>().All(predicate);
        }
    }
}
