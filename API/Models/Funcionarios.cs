using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Models
{
    public class Funcionarios
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public int Idade  { get; set; }
        public bool Empregado { get; set; }
    }
}
