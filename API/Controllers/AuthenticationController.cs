using API.Config;
using API.Models.DTOs.Requests;
using API.Models.DTOs.Respostas;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly JwtConfig _jwtConfig;

        public AuthenticationController(UserManager<IdentityUser>userManager,
              IOptionsMonitor<JwtConfig>options)
        {
            _userManager = userManager;
            _jwtConfig = options.CurrentValue;
        }

        [HttpPost]
        [Route("Register")]

        public async Task<IActionResult> Register([FromBody] RequestCadastro user)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _userManager.FindByEmailAsync(user.Email);

                if (existingUser != null) // User already exists
                {
                    return BadRequest(new RespostaDoRegistro()
                    {
                        Errors = new List<string>()
                        {
                            "Email already in use"
                        },
                        Success = false
                    }) ;
                }
                var newUser = new IdentityUser() { Email = user.Email, UserName = user.UserName };
                var isCreated = await _userManager.CreateAsync(newUser, user.Password);

                if (isCreated.Succeeded)
                {
                    var jwtToken = GenerateJwtToken(newUser);
                    return Ok(new RespostaDoRegistro()
                    {
                        Success = true,
                        Token = jwtToken
                    });
                }

                return BadRequest(new RespostaDoRegistro()
                {
                    Errors = isCreated.Errors.Select(x => x.Description).ToList(),
                    Success = false
                });

            }
            return BadRequest(new RespostaDoRegistro()
            {
                Errors = new List<string>()
                {
                    "Invalid payload"
                },
                Success = false
            });
        }

        [HttpPost]
        [Route("Login")]

        public async Task<IActionResult>Login([FromBody]RequestDoLogin user)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _userManager.FindByEmailAsync(user.Email);
                if (existingUser == null)
                {
                    return BadRequest(new RespostaDoRegistro()
                    {
                        Errors = new List<string>()
                        {
                            "Invalid Login Request"
                        },
                        Success = false
                    });
                }
                var isCorrect = await _userManager.CheckPasswordAsync(existingUser, user.Password);
                if (!isCorrect)
                {
                    return BadRequest(new RespostaDoRegistro()
                    {
                        Errors = new List<string>()
                        {
                            "Invalid login Request"
                        },
                        Success = false
                    });
                }
                var jwtToken = GenerateJwtToken(existingUser);
                return Ok(new RespostaDoRegistro()
                {
                    Success = true,
                    Token = jwtToken
                });
            }
            return BadRequest(new RespostaDoRegistro()
            {
                Errors = new List<string>()
                {
                    "Invalid Login Request"
                },
                Success = false
            });
        }


        private string GenerateJwtToken(IdentityUser user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler(); // gera o token
            var key = Encoding.ASCII.GetBytes(_jwtConfig.Secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] //Assunto da claim
                {
                    new Claim("Id",user.Id),
                    new Claim(JwtRegisteredClaimNames.Email , user.Email),
                    new Claim(JwtRegisteredClaimNames.Sub,user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()) //ID do token
                }),
                Expires = DateTime.UtcNow.AddHours(6),//coloca um tempo de expiração para meu token
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)//assina o token
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = jwtTokenHandler.WriteToken(token);
            return jwtToken;

        }

    }
}
