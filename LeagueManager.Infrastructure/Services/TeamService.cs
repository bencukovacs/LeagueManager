using AutoMapper;
using LeagueManager.Application.Dtos;
using LeagueManager.Application.Services;
using LeagueManager.Domain.Models;
using LeagueManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LeagueManager.Infrastructure.Services;

public class TeamService : ITeamService
{
    private readonly LeagueDbContext _context;
    private readonly IMapper _mapper;

    public TeamService(LeagueDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<TeamResponseDto>> GetAllTeamsAsync()
    {
        var teams = await _context.Teams.ToListAsync();
        return _mapper.Map<IEnumerable<TeamResponseDto>>(teams);
    }

    public async Task<TeamResponseDto?> GetTeamByIdAsync(int id)
    {
        var team = await _context.Teams.FindAsync(id);
        return _mapper.Map<TeamResponseDto>(team);
    }

    public async Task<TeamResponseDto> CreateTeamAsync(CreateTeamDto teamDto)
    {
        var team = _mapper.Map<Team>(teamDto);
        _context.Teams.Add(team);
        await _context.SaveChangesAsync();
        return _mapper.Map<TeamResponseDto>(team);
    }

    public async Task<TeamResponseDto?> UpdateTeamAsync(int id, CreateTeamDto teamDto)
    {
        var team = await _context.Teams.FindAsync(id);
        if (team == null)
        {
            return null;
        }
        _mapper.Map(teamDto, team);
        await _context.SaveChangesAsync();
        return _mapper.Map<TeamResponseDto>(team);
    }

    public async Task<bool> DeleteTeamAsync(int id)
    {
        var team = await _context.Teams.FindAsync(id);
        if (team == null)
        {
            return false;
        }
        var isUsed = await _context.Fixtures.AnyAsync(f => f.HomeTeamId == id || f.AwayTeamId == id);
        if (isUsed)
        {
            throw new InvalidOperationException("Cannot delete a team that is currently assigned to a fixture.");
        }
        _context.Teams.Remove(team);
        await _context.SaveChangesAsync();
        return true;
    }
}