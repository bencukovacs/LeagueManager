using LeagueManager.Domain.Models;

namespace LeagueManager.Application.Dtos;

public class TeamMemberResponseDto
{
    public required string UserId { get; set; }
    public required string Email { get; set; }
    public required string FullName { get; set; }
    public required string Role { get; set; }
}

public class UpdateTeamMemberRoleDto
{
    public TeamRole NewRole { get; set; }
}