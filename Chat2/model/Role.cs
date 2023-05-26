using Microsoft.AspNetCore.Identity;

namespace Chat2.model;

/// <summary>
///     Role contains Id (string) and Name.
/// </summary>
public class Role : IdentityRole
{
    public virtual List<User> Users { get; set; }
}