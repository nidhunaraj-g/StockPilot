
using System.Linq.Expressions;

namespace StockPilot.Application.Common.Interfaces
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        Task<TEntity?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<List<TEntity>> GetAllAsync(
            CancellationToken cancellationToken = default);

        Task<List<TEntity>> GetAllWithIncludesAsync(
            CancellationToken cancellationToken = default,
            params Expression<Func<TEntity, object>>[] includes);

        // New method: loads entities with EF Core tracking enabled.
        Task<List<TEntity>> GetAllWithIncludesTrackedAsync(
            CancellationToken cancellationToken = default,
            params Expression<Func<TEntity, object>>[] includes);

        Task<TEntity?> FindFirstOrDefaultAsync(
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default);

        Task AddAsync(
            TEntity entity,
            CancellationToken cancellationToken = default);

        void Update(TEntity entity);

        void Remove(TEntity entity);

        Task<bool> SaveChangesAsync(
            CancellationToken cancellationToken = default);

        IQueryable<TEntity> GetQueryable();
    }
}
