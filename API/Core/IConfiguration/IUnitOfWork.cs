using API.Core.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Core.IConfiguration
{
    public interface IUnitOfWork
    {
        IFuncionarioRepository Funcionarios { get; }

        IRefreshTokenRepository RefreshTokens { get; }

        Task CompleteAsync();
    }
}
