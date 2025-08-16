using LeagueManager.Application.Dtos;
using LeagueManager.Domain.Models;

namespace LeagueManager.Application.Services;

public interface ITeamService
{
    Task<IEnumerable<TeamResponseDto>> GetAllTeamsAsync();
    Task<IEnumerable<TeamResponseDto>> GetAllTeamsForAdminAsync();
    Task<TeamResponseDto?> GetTeamByIdAsync(int id);
    Task<TeamResponseDto> CreateTeamAsync(CreateTeamDto teamDto);
    Task<TeamResponseDto> CreateTeamAsAdminAsync(CreateTeamDto teamDto);
    Task<TeamResponseDto?> UpdateTeamAsync(int id, CreateTeamDto teamDto);
    Task<bool> DeleteTeamAsync(int id);
    Task<TeamResponseDto?> ApproveTeamAsync(int teamId);
    Task<IEnumerable<TeamResponseDto>> GetPendingTeamsAsync();
    Task<MyTeamAndConfigResponseDto> GetMyTeamAndConfigAsync();
    Task<IEnumerable<FixtureResponseDto>> GetFixturesForMyTeamAsync();
    Task LeaveMyTeamAsync();
}