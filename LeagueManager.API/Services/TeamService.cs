using LeagueManager.API.Data;
using LeagueManager.API.Dtos;
using LeagueManager.API.Models;
using Microsoft.EntityFrameworkCore;

namespace LeagueManager.API.Services;

public class TeamService : ITeamService
{
    private readonly LeagueDbContext _context;

    public TeamService(LeagueDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Team>> GetAllTeamsAsync()
    {
        return await _context.Teams.ToListAsync();
    }

    public async Task<Team?> GetTeamByIdAsync(int id)
    {
        return await _context.Teams.FindAsync(id);
    }

    public async Task<Team> CreateTeamAsync(CreateTeamDto teamDto)
    {
        var team = new Team
        {
            Name = teamDto.Name,
            PrimaryColor = teamDto.PrimaryColor,
            SecondaryColor = teamDto.SecondaryColor
        };

        _context.Teams.Add(team);
        await _context.SaveChangesAsync();
        return team;
    }

    public async Task UpdateTeamAsync(int id, CreateTeamDto teamDto)
    {
        var team = await _context.Teams.FindAsync(id);
        if (team == null)
        {
            // We'll handle the null case in the controller
            return;
        }

        team.Name = teamDto.Name;
        team.PrimaryColor = teamDto.PrimaryColor;
        team.SecondaryColor = teamDto.SecondaryColor;

        _context.Entry(team).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteTeamAsync(int id)
    {
        var team = await _context.Teams.FindAsync(id);
        if (team == null)
        {
            return false;
        }

        // Check if the team is part of any fixture
        var isUsed = await _context.Fixtures.AnyAsync(f => f.HomeTeamId == id || f.AwayTeamId == id);
        if (isUsed)
        {
            // We'll throw an exception that the controller can catch
            throw new InvalidOperationException("Cannot delete a team that is currently assigned to a fixture.");
        }

        _context.Teams.Remove(team);
        await _context.SaveChangesAsync();
        return true;
    }
}