using Microsoft.AspNetCore.Authorization;

namespace LeagueManager.API.Authorization;

public class CanManageTeamRequirement : IAuthorizationRequirement
{
    // This class is empty. Its only purpose is to be a marker
    // that our authorization handler will look for.
}