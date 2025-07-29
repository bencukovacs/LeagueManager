using Microsoft.AspNetCore.Authorization;

namespace LeagueManager.API.Authorization;

public class CanUpdatePlayerRequirement : IAuthorizationRequirement
{
    // This class is intentionally empty. Its purpose is to act as a "marker"
    // for our specific player update authorization policy.
}