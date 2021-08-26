using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Core.IRepositories
{
    public interface IGenericRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetItems();
        Task<T> GetItemById(int id);
        Task<bool> CreateItem(T entidade);
        Task<bool> Delete(int id);
        Task<bool> Update(T entidade);
    }
}
