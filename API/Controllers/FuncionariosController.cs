using API.Config;
using API.Core.IConfiguration;
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
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<FuncionariosController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public FuncionariosController(ILogger<FuncionariosController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetItems()
        {
            var items = await _unitOfWork.Funcionarios.GetItems();            
            return Ok(items);
        }




        [HttpGet("{id}")]
        public async Task<IActionResult> GetItemById(int id)
        {
            var item = await _unitOfWork.Funcionarios.GetItemById(id);
            if (item == null)
            {
                return NotFound(); // codigo 404 do http
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

            await _unitOfWork.Funcionarios.CreateItem(funcionarios);
            await _unitOfWork.CompleteAsync();
            return CreatedAtAction("GetItemById", new { funcionarios.Id }, funcionarios);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFuncionario(int id, Funcionarios funcionario)
        {
            if (id != funcionario.Id)
            {
                return BadRequest();
            }

            await _unitOfWork.Funcionarios.Update(funcionario);
            await _unitOfWork.CompleteAsync();
            return NoContent();

        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFuncionario(int id)
        {
            var funcionarioExistente = await _unitOfWork.Funcionarios.GetItemById(id);
            if (funcionarioExistente == null)
            {
                return BadRequest();
            }

            await _unitOfWork.Funcionarios.Delete(id);
            await _unitOfWork.CompleteAsync();
            return Ok(funcionarioExistente);

        }
    }
}
