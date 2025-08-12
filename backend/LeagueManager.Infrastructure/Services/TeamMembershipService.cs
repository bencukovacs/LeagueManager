using AutoMapper;
using AutoMapper.QueryableExtensions;
using LeagueManager.Application.Dtos;
using LeagueManager.Application.Services;
using LeagueManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LeagueManager.Infrastructure.Services;

public class TeamMembershipService : ITeamMembershipService
{
    private readonly LeagueDbContext _context;
    private readonly IMapper _mapper;

    public TeamMembershipService(LeagueDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<TeamMemberResponseDto>> GetMembersForTeamAsync(int teamId)
    {
        return await _context.TeamMemberships
            .Where(m => m.TeamId == teamId)
            .ProjectTo<TeamMemberResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<TeamMemberResponseDto?> UpdateMemberRoleAsync(int teamId, string userId, UpdateTeamMemberRoleDto dto)
    {
        var membership = await _context.TeamMemberships
            .FirstOrDefaultAsync(m => m.TeamId == teamId && m.UserId == userId);

        if (membership == null)
        {
            return null; // Membership not found
        }

        membership.Role = dto.NewRole;
        await _context.SaveChangesAsync();

        // Fetch the user's name for the response DTO
        var user = await _context.Users.FindAsync(userId);
        var responseDto = _mapper.Map<TeamMemberResponseDto>(membership);
        responseDto.UserName = user?.UserName ?? "Unknown";

        return responseDto;
    }
}