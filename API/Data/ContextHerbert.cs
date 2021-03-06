using API.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Data
{
    public class ContextHerbert :IdentityDbContext<ApplicationUser>
    {
        public virtual DbSet<Funcionarios> Funcionarios{ get; set; }
        public virtual DbSet<RefreshToken> RefreshTokens { get; set; }
        public virtual DbSet<ApplicationUser> ApplicationUser { get; set; }


        public ContextHerbert(DbContextOptions<ContextHerbert>options) : base(options)
        {

        }


    }
}
