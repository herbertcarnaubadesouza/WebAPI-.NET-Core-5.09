using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Config
{
    public class AuthenticationResult //Responsavel pela autenticacao
    {
        public string Token { get; set; }
        public bool Success { get; set; }
        public List<string> Errors { get; set; }
    }
}
