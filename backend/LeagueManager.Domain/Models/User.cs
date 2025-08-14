using Microsoft.AspNetCore.Identity;

namespace LeagueManager.Domain.Models;

// Our User class inherits all the functionality from IdentityUser.
// We can add custom properties here in the future if needed.
public class User : IdentityUser
{
    // Example of a future custom property:
    public required string FullName { get; set; }
}