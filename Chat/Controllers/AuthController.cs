using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Chat.model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WebApplication1;


namespace Chat.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : Controller
{
    private readonly IConfiguration _config;
    private Context _context;
    public UserManager<User> UserManager { get; set; }


    public AuthController(Context context, IConfiguration config,UserManager<User> userManager)
    {
        _context = context;
        _config = config;
        UserManager = userManager;
    }

    private string GenerateToken(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim("sub", user.UserName),
        };
        var token = new JwtSecurityToken(_config["Jwt:Issuer"],
            _config["Jwt:Audience"],
            claims,
            expires: DateTime.Now.AddMinutes(15),
            signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [HttpPost]
    [AllowAnonymous]
    [Route("CreateToken")]
    public async Task<IActionResult> CreateToken(CreateTokenDto createTokenDto)
    {
        var user = await UserManager.FindByNameAsync(createTokenDto.Username);
        if (user != null && await UserManager.CheckPasswordAsync(user, createTokenDto.Password))
        {
            var token = GenerateToken(user);
            return Ok(new {token});
        }

        return Unauthorized();
    }

}

public class CreateTokenDto
{
    public string Username { get; set; }
    public string Password { get; set; }
}

public class UserDto
{
    public string Name { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
}