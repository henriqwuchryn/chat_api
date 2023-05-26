using System.Security.Cryptography;
using System.Text;
using Chat.model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1;


namespace Chat.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController : Controller
{
    private readonly IConfiguration _config;
    private Context _context;
    public UserManager<User> UserManager { get; set; }


    public UsersController(Context context, IConfiguration config, UserManager<User> userManager)
    {
        _context = context;
        _config = config;
        UserManager = userManager;
    }

    [HttpPost]
    [Route("Register")]
    public async Task<IActionResult> Register(UserDto userDto)
    {
        var result = await UserManager.CreateAsync(new User
            {
                Name = userDto.Name,
                UserName = userDto.UserName
            },
            userDto.Password);
        if (result.Succeeded)
        {
            return Ok();
        }

        return Problem(String.Join(",",result.Errors.Select(error => error.Description)));
    }

    public static string ComputeHash(string s)
    {
        var HashByte = new MD5CryptoServiceProvider().ComputeHash(ASCIIEncoding.ASCII.GetBytes(s));
        int i;
        var sOutput = new StringBuilder(HashByte.Length);
        for (i = 0; i < HashByte.Length; i++)
        {
            sOutput.Append(HashByte[i].ToString("X2"));
        }

        return sOutput.ToString();
    }

    [HttpGet]
    [Route("{userId}")]
    public IActionResult GetUserById(int userId)
    {
        var user = _context.Users.Include("Rooms").First(u => u.Id == userId);
        var getUserDto = new GetUserDto
        {
            Name = user.Name,
            UserName = user.UserName,
            Rooms = user.Rooms.Select(room => new UserRoomsDto
            {
                Name = room.Name
            }).ToList()
        };
        return Ok(getUserDto);
    }

    [HttpPatch]
    public IActionResult UpdatePassword(UpdatePasswordDto updatePasswordDto)
    {
        var user = _context.Users.First(u => u.Id == updatePasswordDto.Id);
        var passwordVerificationResult =
            UserManager.PasswordHasher.VerifyHashedPassword(user, user.PasswordHash, updatePasswordDto.OldPassword);
        if (passwordVerificationResult == PasswordVerificationResult.Success ||
            passwordVerificationResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            var newPassword = UserManager.PasswordHasher.HashPassword(user, updatePasswordDto.NewPassword);
            user.PasswordHash = newPassword;
            _context.SaveChanges();
            return Ok();
        }

        return Unauthorized();
    }


    [HttpDelete]
    [Route("{userId}")]
    public async Task<IActionResult> DeleteUser(int userId)
    {
        var user = _context.Users.First(u => u.Id == userId);
        await UserManager.DeleteAsync(user);
        _context.SaveChanges();
        return Ok();
    }

    [HttpGet]
    [Authorize]
    [Route("/me")]
    public IActionResult GetCurrentUser()
    {
        return Ok();
    }
}

public class GetUserDto
{
    public string Name { get; set; }
    public string UserName { get; set; }
    public List<UserRoomsDto> Rooms { get; set; }
}

public class UserRoomsDto
{
    public string Name { get; set; }
}

public class UpdatePasswordDto
{
    public int Id { get; set; }
    public string OldPassword { get; set; }
    public string NewPassword { get; set; }
}