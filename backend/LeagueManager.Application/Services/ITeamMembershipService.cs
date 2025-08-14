using LeagueManager.Application.Dtos;

namespace LeagueManager.Application.Services;

public interface ITeamMembershipService
{
    Task<IEnumerable<TeamMemberResponseDto>> GetMembersForTeamAsync(int teamId);
    Task<TeamMemberResponseDto?> UpdateMemberRoleAsync(int teamId, string memberUserId, UpdateTeamMemberRoleDto dto);
}