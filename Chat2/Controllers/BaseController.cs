using AutoMapper;
using Chat2.model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
namespace Chat2.Controllers.Base;

public class BaseController:Controller
{
    protected readonly UserManager<User> UserManager;

    public BaseController(UserManager<User> userManager)
    {
     UserManager = userManager;
    }
    protected async Task<User> GetUserOrFailAsync()
    {
        var user = await GetUserOrFailAsync() ?? throw new Exception("User is Null");
        return user;
    }
    
    
    
}
