using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Api.FurnitureStore.Api.Configuration;
using Microsoft.Extensions.Options;
using API.FurnitureStore.Shared.DTOs;
using API.FurnitureStore.Shared.Auth;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using System.Text.Encodings.Web;
using System.Security.Cryptography.X509Certificates;
using Api.FurnitureStore.Data;
using API.FurnitureStore.Shared;
using API.FurnitureStore.Shared.Common;
using Microsoft.EntityFrameworkCore;

namespace Api.FurnitureStore.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly JwtConfig _jwtConfig;
        private readonly IEmailSender _emailSender;
        private readonly APIFurnitureStoreContext _context;
        private readonly TokenValidationParameters _tokenValidationParameters;

        public AuthenticationController( UserManager<IdentityUser> userManager , IOptions<JwtConfig> jwtConfig , 
                                          IEmailSender emailSender, APIFurnitureStoreContext context ,
                                          TokenValidationParameters tokenValidationParameters)
        {
            _userManager = userManager;
            _jwtConfig = jwtConfig.Value;
            _emailSender = emailSender;
            _context = context;
            _tokenValidationParameters = tokenValidationParameters;

        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationRequestDto request )
        {
            // verificamos que el request no tenga campos incompletos 
            if (!ModelState.IsValid) return BadRequest();

            // verificamos si el email existe 
            var emailExists = await _userManager.FindByEmailAsync(request.EmailAddress);

            if (emailExists != null )
            {
                return BadRequest(new AuthResult()
                {
                    Result = false,
                    Errors = new List<string>()
                    {
                        " Email already exists"
                    }
                }); 
            }

            // crear usuario 
            var user = new IdentityUser()
            {
                Email = request.EmailAddress,
                UserName = request.EmailAddress,
                EmailConfirmed = false
            };

            var isCreated = await _userManager.CreateAsync(user, request.Password);

            if( isCreated.Succeeded)
            {


                await SendeVerificationEmail(user);

                return Ok(new AuthResult()
                {
                    Result = true

                });


            }
            else
            {
                var errors = new List<string>();

                foreach (var err in isCreated.Errors)
                    errors.Add(err.Description);

                return BadRequest(new AuthResult()
                {
                    Result = false,
                    Errors = errors
                });
            

            }

            return BadRequest(new AuthResult()
            {
                Result =false,
                Errors = new List<string>() { " User couldn't be created"}
            });
        }
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequestDto request)
        {
            if (!ModelState.IsValid) return BadRequest();
            // verificamos que el usuario existe 
            var existingUser = await _userManager.FindByEmailAsync(request.Email);

            if(existingUser == null)
            {
                return BadRequest(new AuthResult
                {
                    Errors = new List<string>()
                    {
                        "Invalid Paylod"
                    } ,
                    Result = false
                });   
            }
            if(!existingUser.EmailConfirmed)
            {
                return BadRequest(new AuthResult
                {
                    Errors = new List<string>()
                    {
                        "Necesitamos que por favor confirme el email"
                    },
                    Result = false
                });
            }


            // verificamos que la contrasena sea correcta 
            var checkUserAndPass = await _userManager.CheckPasswordAsync(existingUser, request.Password);

            if (!checkUserAndPass)
            {
                return BadRequest(new AuthResult
                {
                    Errors = new List<string> { "Invalid Credentials" },
                    Result = false
                });

            }
            var token = GenerateToken(existingUser);

            return Ok(token);
        }

        [HttpPost("RefreshToken")]

        public async Task<IActionResult> RefreshToken([FromBody] TokenRequst tokenRequest)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(new AuthResult
                {
                    Errors = new List<string> { "Invalid Parameters"},
                    Result = false
                });
            }

            var result = VerifyAndGenerateTokenAsync(tokenRequest);

            if (result == null)
            {
                return BadRequest(new AuthResult { Errors = new List<string> { "Invalid token" } });
            }
            return Ok(result);
        }

        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail( string userId , string code )
        {
            if( string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(code))
            {
                return BadRequest(new AuthResult
                {
                    Errors = new List<string>() { "Invalid email confirmation url"},
                    Result = false
                });
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound($"Unable to load user with id '{userId}' .");

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));

            var result = await _userManager.ConfirmEmailAsync(user, code);

            var status = result.Succeeded ? "Thank you for confirming your email." : "there has been error confirming your email.";

            return Ok(status);
        }

        private async Task<AuthResult>  GenerateToken( IdentityUser user)
        {
            var JwtTokenHandeler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtConfig.Secret);

            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new ClaimsIdentity(new[]
                {
                    new Claim("Id" , user.Id),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTime.Now.ToUniversalTime().ToString() )
                })),
                Expires = DateTime.UtcNow.Add(_jwtConfig.ExparyTime),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)//va
            };
            var token = JwtTokenHandeler.CreateToken(tokenDescriptor);

                var jwtToken = JwtTokenHandeler.WriteToken(token);

            var refreshToken = new RefreshToken
            {
                JwtId = token.Id,
                Token = RandomGenerator.GenerateRandomString(30),
                AddedDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddDays(30),
                IsRevoked = false,
                IsUsed = false,
                UserId = user.Id
            };
            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();

            return new AuthResult
            {
                Token = jwtToken,
                RefreshToken = refreshToken.Token,
                Result = true
            };
        }


        private  async Task SendeVerificationEmail( IdentityUser user)
        {
            // greneracion de verificacion y crearemos una callback Url

            var verificationCode = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            verificationCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(verificationCode));

            //example : https://localhost:8080/api/authentication/verifyemail/userId=exampleuser&code=examplecode
            var callbackUrl = $@"{Request.Scheme}://{Request.Host}{Url.Action("ConfirmEmail", controller: "Authentication",
                                    new { userId = user.Id, code = verificationCode })}";

            var emailBody = $"Please confirm your account by <a href ='{HtmlEncoder.Default.Encode(callbackUrl)}'> Clicking Here </a>";

            await _emailSender.SendEmailAsync(user.Email , "Confirm your email", emailBody);
        }

        private async Task<AuthResult> VerifyAndGenerateTokenAsync( TokenRequst tokenRequest)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            try
            {
                // validacion de access token 
                _tokenValidationParameters.ValidateLifetime = false;
                var tokenBeingVerified = jwtTokenHandler.ValidateToken(tokenRequest.Token, _tokenValidationParameters, out var ValidatedToken);
                
                if (ValidatedToken is  JwtSecurityToken jwtSecurityToken)
                {
                    var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);

                    if ( !result || tokenBeingVerified == null)
                    {
                        throw new Exception("Invalid Token");
                    }
                }
                var UtcExpiryDate = long.Parse(tokenBeingVerified.Claims.FirstOrDefault(p => p.Type == JwtRegisteredClaimNames.Exp).Value);

                var expiryDate = DateTimeOffset.FromUnixTimeSeconds(UtcExpiryDate).UtcDateTime;

                if (expiryDate < DateTime.UtcNow)
                    throw new Exception("Token Expired");

                // validacion del Refresh Token 

                var storedToken = await _context.RefreshTokens.FirstOrDefaultAsync(p => p.Token == tokenRequest.RefreshToeken);

                if (storedToken == null)
                    throw new Exception("invalid Token");

                if (storedToken.IsRevoked || storedToken.IsUsed)
                    throw new Exception("invalid Token ");

                var jti = tokenBeingVerified.Claims.FirstOrDefault(p => p.Type == JwtRegisteredClaimNames.Jti).Value;

                if (jti != storedToken.JwtId)
                    throw new Exception("invalid Token");

                if (storedToken.ExpiryDate < DateTime.UtcNow)
                    throw new Exception("Token Expired");
                storedToken.IsUsed = true;
                _context.RefreshTokens.Update(storedToken);

                await _context.SaveChangesAsync();

                var dbUser = await _userManager.FindByIdAsync(storedToken.UserId);

                return await GenerateToken(dbUser);




            }
            catch(Exception e)
            {
                var message = e.Message == "invalid Token" || e.Message == "Token Expired" ? e.Message : "Internal Server Error ";

                return new AuthResult() { Result = false, Errors = new List<string> { message } };
            }
        }

    }
}
