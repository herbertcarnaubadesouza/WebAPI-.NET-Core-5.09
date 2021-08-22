using API.Config;
using API.Data;
using API.Models;
using API.Models.DTOs.Requests;
using API.Models.DTOs.Respostas;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace API.Controllers
{
    [Route("api/[controller]")] //api/funcionarios
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class FuncionariosController : ControllerBase
    {
        private readonly ContextHerbert _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly JwtConfig _jwtConfig;

        public FuncionariosController(ContextHerbert context, UserManager<IdentityUser> userManager,
            IOptionsMonitor<JwtConfig> options)
        {
            _context = context;
            _userManager = userManager;
            _jwtConfig = options.CurrentValue;
        }

        

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetItems()
        {
            var items = await _context.Funcionarios.ToListAsync();            
            return Ok(items);
        }




        [HttpGet("{id}")]
        public async Task<IActionResult> GetItemById(int id)
        {
            var item = await _context.Funcionarios.FirstOrDefaultAsync(x => x.Id == id);
            if (item == null)
            {
                return NotFound();
            }

            return Ok(item);
        }
        //post
        [HttpPost]
        public async Task<IActionResult> CreateFuncionario(Funcionarios funcionarios)
        {
            if (!ModelState.IsValid)
            {
                return new JsonResult("Something Went Wrong") { StatusCode = 500 };
            }

            await _context.Funcionarios.AddAsync(funcionarios);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetItemById", new { funcionarios.Id }, funcionarios);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFuncionario(int id, Funcionarios funcionario)
        {
            if (id != funcionario.Id)
            {
                return BadRequest();
            }

            var funcionarioExiste = await _context.Funcionarios.FirstOrDefaultAsync(x => x.Id == id);
            if (funcionarioExiste == null)
            {
                return NotFound();
            }
            funcionarioExiste.Id = funcionario.Id;
            funcionarioExiste.Nome = funcionario.Nome;
            funcionarioExiste.Idade = funcionario.Idade;
            funcionarioExiste.Empregado = funcionario.Empregado;

            await _context.SaveChangesAsync();
            return NoContent();

        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFuncionario(int id)
        {
            var funcionarioExistente = await _context.Funcionarios.FirstOrDefaultAsync(x => x.Id == id);
            if (funcionarioExistente == null)
            {
                return NotFound();
            }

           _context.Funcionarios.Remove(funcionarioExistente);
            await _context.SaveChangesAsync();
            return Ok(funcionarioExistente);

        }
    }
}
