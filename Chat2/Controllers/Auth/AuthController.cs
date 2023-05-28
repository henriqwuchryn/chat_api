using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Chat2.model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Chat2.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : Controller
{
    private readonly IConfiguration _config;
    private Context _context;


    public AuthController(Context context, IConfiguration config, UserManager<User> userManager)
    {
        _context = context;
        _config = config;
        UserManager = userManager;
    }

    public UserManager<User> UserManager { get; set; }

    [HttpPost]
    [Route("Register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(LoginDto loginDto)
    {
        var result = await UserManager.CreateAsync(new User
            {
                UserName = loginDto.UserName
            },
            loginDto.Password
        );
        if (result.Succeeded)
            return Ok();

        return Problem(string.Join(";", result.Errors.Select(error => error.Description)));
    }

    [HttpPost]
    [AllowAnonymous]
    [Route("Login")]
    public async Task<IActionResult> Login(LoginDto loginDto)
    {
        var user = await UserManager.FindByNameAsync(loginDto.UserName);
        if (user != null && await UserManager.CheckPasswordAsync(user, loginDto.Password))
        {
            var token = await GenerateTokenAsync(user);
            return Ok(new { token });
        }

        return Unauthorized();
    }

    private async Task<string> GenerateTokenAsync(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        //symmetric security key means it is the same to encrypt as it is to read
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName)
            }),
            Audience = _config["Jwt:Audience"],
            Issuer = _config["Jwt:Issuer"],
            Expires = DateTime.Now.AddHours(5),
            SigningCredentials = credentials
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}