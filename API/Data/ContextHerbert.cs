using API.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Data
{
    public class ContextHerbert :IdentityDbContext
    {
        public virtual DbSet<Funcionarios> Funcionarios{ get; set; }


        public ContextHerbert(DbContextOptions<ContextHerbert>options) : base(options)
        {

        }


    }
}
