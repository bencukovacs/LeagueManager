using AutoMapper;
using AutoMapper.QueryableExtensions;
using LeagueManager.Application.Dtos;
using LeagueManager.Application.Services;
using LeagueManager.Domain.Models;
using LeagueManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LeagueManager.Infrastructure.Services;

public class FixtureService : IFixtureService
{
    private readonly LeagueDbContext _context;
    private readonly IMapper _mapper;

    public FixtureService(LeagueDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<FixtureResponseDto>> GetAllFixturesAsync()
    {
        return await _context.Fixtures
            .ProjectTo<FixtureResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<FixtureResponseDto?> GetFixtureByIdAsync(int id)
    {
        return await _context.Fixtures
            .Where(f => f.Id == id)
            .ProjectTo<FixtureResponseDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<FixtureResponseDto> CreateFixtureAsync(CreateFixtureDto fixtureDto)
    {
        if (fixtureDto.HomeTeamId == fixtureDto.AwayTeamId)
        {
            throw new ArgumentException("Home team and away team cannot be the same.");
        }

        var teamsExist = await _context.Teams.CountAsync(t => t.Id == fixtureDto.HomeTeamId || t.Id == fixtureDto.AwayTeamId);
        if (teamsExist != 2)
        {
            throw new ArgumentException("One or both teams do not exist.");
        }

        if (fixtureDto.LocationId.HasValue)
        {
            var locationExists = await _context.Locations.AnyAsync(l => l.Id == fixtureDto.LocationId.Value);
            if (!locationExists)
            {
                throw new ArgumentException("The specified location does not exist.");
            }
        }

        var fixture = new Fixture
        {
            HomeTeamId = fixtureDto.HomeTeamId,
            AwayTeamId = fixtureDto.AwayTeamId,
            KickOffDateTime = fixtureDto.KickOffDateTime,
            LocationId = fixtureDto.LocationId,
            Status = FixtureStatus.Scheduled
        };

        _context.Fixtures.Add(fixture);
         return await GetFixtureByIdAsync(fixture.Id) 
               ?? throw new InvalidOperationException("Could not retrieve created fixture.");
    }

    public async Task<FixtureResponseDto?> UpdateFixtureAsync(int id, UpdateFixtureDto fixtureDto)
    {
        var fixture = await _context.Fixtures.FindAsync(id);
        if (fixture == null)
        {
            return null;
        }
        
        if (fixtureDto.LocationId.HasValue)
        {
            var locationExists = await _context.Locations.AnyAsync(l => l.Id == fixtureDto.LocationId.Value);
            if (!locationExists)
            {
                throw new ArgumentException("The specified location does not exist.");
            }
        }

        fixture.KickOffDateTime = fixtureDto.KickOffDateTime;
        fixture.LocationId = fixtureDto.LocationId;

        _context.Entry(fixture).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return _mapper.Map<FixtureResponseDto>(fixture);
    }

    public async Task<bool> DeleteFixtureAsync(int id)
    {
        var fixture = await _context.Fixtures.FindAsync(id);
        if (fixture == null)
        {
            return false;
        }
        _context.Fixtures.Remove(fixture);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Result> SubmitResultAsync(int fixtureId, SubmitResultDto resultDto)
    {
        var fixture = await _context.Fixtures.FindAsync(fixtureId)
            ?? throw new KeyNotFoundException("Fixture not found.");

        if (await _context.Results.AnyAsync(r => r.FixtureId == fixtureId))
        {
            throw new InvalidOperationException("A result for this fixture has already been submitted.");
        }

        var totalGoals = resultDto.Goalscorers.Count;
        if (totalGoals != resultDto.HomeScore + resultDto.AwayScore)
        {
            throw new InvalidOperationException("The number of goalscorers does not match the total score.");
        }

        var playerIds = resultDto.Goalscorers.Select(g => g.PlayerId).ToList();
        if (playerIds.Any())
        {
            var validPlayersCount = await _context.Players
                .CountAsync(p => playerIds.Contains(p.Id) && (p.TeamId == fixture.HomeTeamId || p.TeamId == fixture.AwayTeamId));

            if (validPlayersCount != playerIds.Count)
            {
                throw new InvalidOperationException("One or more goalscorer IDs are invalid or do not belong to the competing teams.");
            }
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        var result = new Result
        {
            FixtureId = fixtureId,
            HomeScore = resultDto.HomeScore,
            AwayScore = resultDto.AwayScore,
            Status = ResultStatus.PendingApproval
        };
        _context.Results.Add(result);

        fixture.Status = FixtureStatus.Completed;
        _context.Fixtures.Update(fixture);

        foreach (var goalscorer in resultDto.Goalscorers)
        {
            var goal = new Goal
            {
                PlayerId = goalscorer.PlayerId,
                FixtureId = fixtureId
            };
            _context.Goals.Add(goal);
        }

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return result;
    }
}