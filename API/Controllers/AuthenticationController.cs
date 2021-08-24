using API.Config;
using API.Data;
using API.Models;
using API.Models.DTOs.Requests;
using API.Models.DTOs.Respostas;
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
    [Route("api/[controller]")]
    [ApiController]
   
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        
        private readonly JwtConfig _jwtConfig;
        private readonly TokenValidationParameters _tokenValidationParams;
        private readonly ContextHerbert _contextHerbert;
        

        public AuthenticationController(UserManager<IdentityUser>userManager,
              TokenValidationParameters tokenValidationParams,
              ContextHerbert contextHerbert,
              IOptionsMonitor<JwtConfig>options)
        {
            _userManager = userManager;
            _tokenValidationParams = tokenValidationParams;
            _contextHerbert = contextHerbert;
            _jwtConfig = options.CurrentValue;
        }

        [HttpPost]
        [Route("Register")]
        //POST : /api/ApplicationUser/Register

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
                    var jwtToken = await GenerateJwtToken(newUser);
                    return Ok(jwtToken);
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
                var jwtToken = await GenerateJwtToken(existingUser);
                return Ok(jwtToken);
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

        [HttpPost]
        [Route("RefreshToken")]

        public async Task<IActionResult> RefreshToken([FromBody] TokenRequest tokenRequest)
        {
            if (ModelState.IsValid)
            {
               var result = await VerifyTokenAndGenerate(tokenRequest);

                if (result == null)
                {
                    return BadRequest(new RespostaDoRegistro()
                    {
                        Errors = new List<string>()
                        {
                            "Invalid Tokens"
                        },
                        Success = false
                    });
                }

                return Ok(result);
            
            }
            return BadRequest(new RespostaDoRegistro()
            {
                Errors = new List<string>()
                {"Invalid Payload"

                },
                Success = false
            });
        }


        private async Task<AuthenticationResult> GenerateJwtToken(IdentityUser user)
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
                    new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()), //ID do token
                    
                }),
                Expires = DateTime.UtcNow.AddSeconds(30),//coloca um tempo de expiração para meu token
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)//assina o token
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = jwtTokenHandler.WriteToken(token);

            var refreshToken = new RefreshToken()
            {
                JwtId = token.Id,
                IsUsed = false,
                IsRevorked = false,
                UserId = user.Id,
                AddedDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddMonths(6),
                Token = RandomString(35) + Guid.NewGuid()
            };

            await _contextHerbert.RefreshTokens.AddAsync(refreshToken);
            await _contextHerbert.SaveChangesAsync();

            return new AuthenticationResult() 
            {
                Token = jwtToken,
                Success = true,
                RefreshToken = refreshToken.Token
            };

        }


        private async Task<AuthenticationResult>VerifyTokenAndGenerate(TokenRequest tokenRequest)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            try
            {
                //Validation 1 - Validation JWT token format
                var tokenInVerification = jwtTokenHandler.ValidateToken(tokenRequest.Token,
                    _tokenValidationParams, out var validatedToken);
                //Validation 2 - Validate encryption algorinthimun
                if (validatedToken is JwtSecurityToken jwtSecurityToken)
                {
                    var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);

                    if (result == false)
                    {
                        return null;
                    }                
                }

                //Validation 3 - validate expiry date
                var utcExpiryDate = long.Parse(tokenInVerification.Claims.FirstOrDefault(x =>//long pq vem em segundos, muito grande
                    x.Type == JwtRegisteredClaimNames.Exp).Value);//firstOrDefault para enconstrar o Exp(tempo de expiração)

                var expiryDate = UnixTimeStampToDateTime(utcExpiryDate);

                if (expiryDate > DateTime.UtcNow) // Se for maior, ainda não expirou, nao precisa gerar outro
                {
                    return new AuthenticationResult()
                    {
                        Success = false,
                        Errors = new List<string>()
                        {
                            "Token ainda não expirou"
                        }
                    };
                }
                //Validation 4 Verificar se existe o token
                var storedToken = await _contextHerbert.RefreshTokens.FirstOrDefaultAsync(x =>
                    x.Token == tokenRequest.RefreshToken);


                if (storedToken == null)
                {
                    return new AuthenticationResult()
                    {
                        Success = false,
                        Errors = new List<string>()
                        {
                            "Token não existe"
                        }
                    };
                }

                //Validation 5 Verificar se ja foi usado

                if (storedToken.IsUsed)
                {
                    return new AuthenticationResult()
                    {
                        Success = false,
                        Errors = new List<string>()
                        {
                            "Token já foi usado"
                        }
                    };
                }

                //Validation 6 - verifica se foi anulado

                if (storedToken.IsRevorked)
                {
                    return new AuthenticationResult()
                    {
                        Success = false,
                        Errors = new List<string>()
                        {
                            "Token foi anulado"
                        }
                    };
                }
                //Validation 7 - validate the Id
                var jti = tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;

                if (storedToken.JwtId != jti)
                {
                    return new AuthenticationResult()
                    {
                        Success = false,
                        Errors = new List<string>()
                        {
                            "Token doesnt match"
                        }
                    };
                }

                // update current token

                storedToken.IsUsed = true;
                _contextHerbert.RefreshTokens.Update(storedToken);
                await _contextHerbert.SaveChangesAsync();

                //Generate a new token
                var dbUser = await _userManager.FindByIdAsync(storedToken.UserId);
                return await GenerateJwtToken(dbUser);

            }
            catch(Exception e)
            {
                return null;
            }
        }

        private DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            var dateTimeVal = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTimeVal = dateTimeVal.AddSeconds(unixTimeStamp).ToUniversalTime();
            return dateTimeVal;
        }


        private string RandomString(int length)
        {
            var random = new Random();
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(x => x[random.Next(x.Length)]).ToArray());
        }

    }
}
