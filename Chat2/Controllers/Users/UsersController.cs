using System.Security.Cryptography;
using System.Text;
using Chat2.Controllers.Base;
using Chat2.model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Chat2.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController : Controller
{
    private readonly IConfiguration _config;
    private readonly Context _context;
    public UserManager<User> UserManager { get; set; }

    public UsersController(Context context, IConfiguration config, UserManager<User> userManager)
    {
        _context = context;
        _config = config;
        UserManager = userManager;
    }
   
    
    [HttpGet]
    [Route("{userId}")]
    public IActionResult GetUserByUserName(string UserName)
    {
        var user = _context.Users.Include("Rooms").First(u => u.UserName == UserName);
        var getUserDto = new GetUserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Rooms = user.Rooms.Select(room => new UserRoomsDto
            {
                Name = room.Name
            }).ToList()
        };
        return Ok(getUserDto);
    }

    [HttpPatch]
    [Authorize]
    public async Task<IActionResult> UpdatePassword(UpdatePasswordDto updatePasswordDto)
    {
        var user = await UserManager.GetUserAsync(User);
        //_context.Users.First(u => u.UserName == updatePasswordDto.UserName); removido
        var passwordVerificationResult =
            UserManager.PasswordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                updatePasswordDto.OldPassword);
        if (passwordVerificationResult == PasswordVerificationResult.Success ||
            passwordVerificationResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            var newPassword = UserManager.PasswordHasher.HashPassword(user, updatePasswordDto.NewPassword);
            user.PasswordHash = newPassword;
            await UserManager.UpdateAsync(user);
                //_context.SaveChangesAsync(); removido
            return Ok();
        }

        return Unauthorized();
    }


    [HttpDelete]
    [Authorize]
    [Route("{userId}")]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        var user = await UserManager.GetUserAsync(User);
        await UserManager.DeleteAsync(user);
        await UserManager.UpdateAsync(user);
        return Ok();
    }

    [HttpGet]
    [Authorize]
    [Route("/me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var user = await UserManager.GetUserAsync(User);
        return Ok(user);
    }
}

public class UserDetailsDto:EntityDto
{
    public string UserName { get; set; }
   }

public class GetUserDto
{
    public string Id { get; set; }
    public string UserName { get; set; }
    public List<UserRoomsDto> Rooms { get; set; }
}

public class UserRoomsDto

{
    public string Name { get; set; }
}

public class UpdatePasswordDto
{
    public string OldPassword { get; set; }
    public string NewPassword { get; set; }
}