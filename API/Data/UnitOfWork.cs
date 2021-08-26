using API.Core.IConfiguration;
using API.Core.IRepositories;
using API.Core.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Data
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly ContextHerbert _context;
        private readonly ILogger _logger;
        public IFuncionarioRepository Funcionarios { get; private set; }

        public UnitOfWork(ContextHerbert context,
            ILoggerFactory loggerFactory)
        {
            _context = context;
            _logger = loggerFactory.CreateLogger("logs");

            Funcionarios = new FuncionarioRepository(_context, _logger);
        }
        public IRefreshTokenRepository RefreshTokens => throw new NotImplementedException();

        public async Task CompleteAsync()
        {
            await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }

    }
}
