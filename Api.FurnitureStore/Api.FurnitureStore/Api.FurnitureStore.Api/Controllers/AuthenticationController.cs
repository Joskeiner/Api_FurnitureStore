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

namespace Api.FurnitureStore.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly JwtConfig _jwtConfig;
        private readonly IEmailSender _emailSender;

        public AuthenticationController( UserManager<IdentityUser> userManager ,
                                          IOptions<JwtConfig> jwtConfig , 
                                          IEmailSender emailSender)
        {
            _userManager = userManager;
            _jwtConfig = jwtConfig.Value;
            _emailSender = emailSender;

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
                // var token = GenerateToken(user);

                await SendeVerificationEmail(user);
                return Ok(new AuthResult()
                {
                    Result = true
                    //Token = token
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

            return Ok(new AuthResult { Token = token, Result = true });
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

        private string  GenerateToken( IdentityUser user)
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
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };
            var token = JwtTokenHandeler.CreateToken(tokenDescriptor);

            return JwtTokenHandeler.WriteToken(token);
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


    }
}
