using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System;
using System.Threading.Tasks;
using WebsocketChat.Library.Entities;
using WebsocketChat.Server.Contexts;

namespace WebsocketChat.Server.Services
{
    public abstract class BaseDbService<TEntity>
        where TEntity: class, IEntity
    {
        protected AppIdentityDbContext _context;

        protected async Task<string> CreateEntityAsync(TEntity item,
            CancellationToken cancellationToken = default)
        {
            _context.Set<TEntity>().Add(item);
            await _context.SaveChangesAsync(cancellationToken);

            return item.Id;
        }

        public IQueryable<TEntity> GetAll()
        {
            return _context.Set<TEntity>().AsNoTracking();
        }

    }
}
