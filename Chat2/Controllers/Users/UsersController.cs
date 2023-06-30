using AutoMapper;
using Chat2.Controllers.Base;
using Chat2.model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Chat2.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController : BaseController
{
    private readonly IConfiguration _config;
    private readonly Context _context;
    private readonly IMapper _mapper;

    public UsersController(
        Context context,
        IConfiguration config,
        UserManager<User> userManager,
        IMapper mapper) : base(userManager, context)
    {
        _context = context;
        _config = config;
        _mapper = mapper;
    }

    //User creation is handled by AuthController

    [HttpGet]
    [Route("{userId}")]
    public IActionResult GetUser(string userId)
    {
        var user = _context.Users.Include("Rooms").First(u => u.Id == userId);
        var getUserDto = _mapper.Map<UserDetailsDto>(user);
        return Ok(getUserDto);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var userList = UserManager.Users.ToList();
        var userListItemList = userList.Select(t => _mapper.Map<UserListItemDto>(t)).ToList();
        return Ok(userListItemList);
    }


    [HttpPatch]
    [Authorize]
    [Route("/Me/Password")]
    public async Task<IActionResult> UpdatePassword(UpdatePasswordDto updatePasswordDto)
    {
        var user = await GetUserOrFailAsync();
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
    [Route("/Me/User/{userId}")]
    public async Task<IActionResult> DeleteUser()
    {
        var user = await GetUserOrFailAsync();
        await UserManager.DeleteAsync(user);
        await UserManager.UpdateAsync(user);
        return Ok();
    }

    [HttpGet]
    [Authorize]
    [Route("/Me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var user = await GetUserOrFailAsync();
        var userDetailsDto = _mapper.Map<UserDetailsDto>(user);
        return Ok(userDetailsDto);
    }
}