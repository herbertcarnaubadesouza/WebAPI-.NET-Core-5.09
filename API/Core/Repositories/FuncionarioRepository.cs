using API.Core.IRepositories;
using API.Data;
using API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Core.Repositories
{
    public class FuncionarioRepository : GenericRepository<Funcionarios>, IFuncionarioRepository
    {
        public FuncionarioRepository(ContextHerbert context, ILogger logger) : base(context, logger) { }

        public override async Task<IEnumerable<Funcionarios>> GetItems()
        {
            try
            {
                return await dbSet.ToListAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "{Repo} All method error", typeof(FuncionarioRepository));
                return new List<Funcionarios>();
                
            }
        }

        public override async Task<bool> Update(Funcionarios entidade)
        {
            try
            {
                var existingUser = await dbSet.Where(x => x.Id == entidade.Id).FirstOrDefaultAsync();
                if (existingUser == null)
                {
                    return await CreateItem(entidade);
                }
               
                existingUser.Idade = entidade.Idade;
                existingUser.Nome = entidade.Nome;
                existingUser.Empregado = entidade.Empregado;

                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "{Repo} Update method error", typeof(FuncionarioRepository));
                return false;
            }
        }

        public override async Task<bool> Delete(int id)
        {
            try
            {
                var exist = await dbSet.Where(x => x.Id == id).FirstOrDefaultAsync();
                if (exist != null)
                {
                    dbSet.Remove(exist);
                    return true;
                }
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "{Repo} Delete method error", typeof(FuncionarioRepository));
                return false;
            };
        }


    }
}
