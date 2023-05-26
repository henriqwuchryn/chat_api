using Microsoft.AspNetCore.Identity;

namespace Chat.model;

public class Role:IdentityRole
{
    public virtual List<User> Users { get; set; }
}