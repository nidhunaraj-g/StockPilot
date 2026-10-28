using Microsoft.EntityFrameworkCore;
using StockPilot.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Infrastructure.Persistence.Repositories
{
    public class GenericRepository<TEntity>(StockPilotDbContext context) : IGenericRepository<TEntity> where TEntity : class
    {
        protected readonly StockPilotDbContext Context = context;

        protected readonly DbSet<TEntity> DbSet =
            context.Set<TEntity>();

        public async Task<TEntity?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return await DbSet.FindAsync(
                [id],
                cancellationToken);
        }

        public async Task<List<TEntity>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            return await DbSet
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(
            TEntity entity,
            CancellationToken cancellationToken = default)
        {
            await DbSet.AddAsync(entity, cancellationToken);
        }

        public void Update(TEntity entity)
        {
            DbSet.Update(entity);
        }

        public void Remove(TEntity entity)
        {
            DbSet.Remove(entity);
        }

        public async Task<bool> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            return await Context.SaveChangesAsync(cancellationToken) > 0;
        }
        public async Task<TEntity?> FindFirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate,CancellationToken cancellationToken = default)
        {
            return await DbSet.FirstOrDefaultAsync(
                predicate,
                cancellationToken);
        }
    }
}
