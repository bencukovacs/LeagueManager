using LeagueManager.Application.Dtos;
using LeagueManager.Domain.Models;

namespace LeagueManager.Application.Services;

public interface IRosterRequestService
{
  Task<RosterRequestResponseDto> CreateJoinRequestAsync(int teamId);
  Task<IEnumerable<RosterRequestResponseDto>> GetPendingJoinRequestsForMyTeamAsync();
  Task<TeamMembership> ApproveJoinRequestAsync(int requestId);
  Task RejectJoinRequestAsync(int requestId);
}