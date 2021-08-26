using API.Core.IRepositories;
using API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Core.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected ContextHerbert _context;
        protected DbSet<T> dbSet;
        protected readonly ILogger _logger;

        public GenericRepository(ContextHerbert context, ILogger logger)
        {
            _context = context;            
            _logger = logger;
            dbSet = context.Set<T>();
        }

        public virtual async Task<bool> CreateItem(T entidade)
        {
            await dbSet.AddAsync(entidade);
            return true;
        }

        public virtual Task<bool> Delete(int id)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<T> GetItemById(int id)
        {
            return await dbSet.FindAsync(id);
        }

        public virtual async Task<IEnumerable<T>> GetItems()
        {
            return await dbSet.ToListAsync();
        }

        public virtual Task<bool> Update(T entidade)
        {
            throw new NotImplementedException();
        }
    }
}
