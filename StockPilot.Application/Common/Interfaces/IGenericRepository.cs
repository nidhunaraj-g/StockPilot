using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Application.Common.Interfaces
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        Task<TEntity?> GetByIdAsync(int id,CancellationToken cancellationToken = default);

        Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<List<TEntity>> GetAllWithIncludesAsync(CancellationToken cancellationToken = default,params Expression<Func<TEntity, object>>[] includes);

        Task<TEntity?> FindFirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate,CancellationToken cancellationToken = default);

        Task AddAsync(TEntity entity,CancellationToken cancellationToken = default);

        void Update(TEntity entity);

        void Remove(TEntity entity);

        Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default);
        IQueryable<TEntity> GetQueryable();
    }
}

