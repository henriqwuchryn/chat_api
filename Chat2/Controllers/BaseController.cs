using AutoMapper;
using Chat2.model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Chat2.Controllers.Base;

public class BaseController : Controller
{
    protected readonly UserManager<User> UserManager;
    protected readonly Context Context;

    public BaseController(UserManager<User> userManager, Context context)
    {
        UserManager = userManager;
        Context = context;
    }

    protected async Task<User> GetUserOrFailAsync()
    {
        var user = await UserManager.GetUserAsync(User) ?? throw new Exception("User is Null");
        var userContext = await Context.Users.FindAsync(user.Id) ?? throw new Exception("User is Null");
        return userContext;
    }
}