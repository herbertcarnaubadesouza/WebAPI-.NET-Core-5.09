using API.Migrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Models;

namespace API.Core.IRepositories
{
    public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
    {
    }
}
