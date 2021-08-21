using API.Data;
using API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Controllers
{
    [Route("api/[controller]")] //api/funcionarios
    [ApiController]
    public class FuncionariosController : ControllerBase
    {
        private readonly ContextHerbert _context;

        public FuncionariosController(ContextHerbert context)
        {
            _context = context;
        }
        
        [HttpGet]
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
