using AutoMapper;
using AutoMapper.QueryableExtensions;
using LeagueManager.Application.Dtos;
using LeagueManager.Application.Services;
using LeagueManager.Domain.Models;
using LeagueManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace LeagueManager.Infrastructure.Services;

public class FixtureService : IFixtureService
{
    private readonly LeagueDbContext _context;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public FixtureService(LeagueDbContext context, IMapper mapper, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IEnumerable<FixtureResponseDto>> GetAllFixturesAsync()
    {
        return await _context.Fixtures
            .ProjectTo<FixtureResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<FixtureResponseDto?> GetFixtureByIdAsync(int id)
    {
        // We can't use ProjectTo here because the roster logic is too complex for it to figure out.
        // We'll fetch the full entity and then map it.
        var fixture = await _context.Fixtures
            .Include(f => f.HomeTeam).ThenInclude(t => t.Players)
            .Include(f => f.AwayTeam).ThenInclude(t => t.Players)
            .Include(f => f.Location)
            .Include(f => f.Result)
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == id);

        if (fixture == null) return null;

        if (fixture.HomeTeam == null) return null;
        if (fixture.AwayTeam == null) return null;

        var fixtureDto = _mapper.Map<FixtureResponseDto>(fixture);

        // Manually map the rosters
        fixtureDto.HomeTeamRoster = _mapper.Map<List<PlayerResponseDto>>(fixture.HomeTeam.Players);
        fixtureDto.AwayTeamRoster = _mapper.Map<List<PlayerResponseDto>>(fixture.AwayTeam.Players);

        return fixtureDto;
    }

    public async Task<IEnumerable<MomVoteResponseDto>> GetMomVotesForFixtureAsync(int fixtureId)
    {
        var votes = await _context.MOMVotes
            .Where(v => v.FixtureId == fixtureId)
            .Include(v => v.VotingTeam)
            .Include(v => v.VotedForOwnPlayer)
            .Include(v => v.VotedForOpponentPlayer)
            .ToListAsync();

        return _mapper.Map<IEnumerable<MomVoteResponseDto>>(votes);
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
            KickOffDateTime = fixtureDto.KickOffDateTime.ToUniversalTime(),
            LocationId = fixtureDto.LocationId,
            Status = FixtureStatus.Scheduled
        };

        _context.Fixtures.Add(fixture);
        await _context.SaveChangesAsync();

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
        return await GetFixtureByIdAsync(id);
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
    
    public async Task<ResultResponseDto> SubmitResultAsync(int fixtureId, SubmitResultDto resultDto)
    {
        var fixture = await GetFixtureAsync(fixtureId);
        var currentUserId = GetCurrentUserId();

        await EnsureNoExistingResultAsync(fixtureId);
        ValidateGoalCount(resultDto);
        await ValidateGoalscorersAsync(resultDto, fixture);

        if (resultDto.MomVote != null)
        {
            await ValidateMomVoteAsync(resultDto, fixture, currentUserId);
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        var result = await CreateResultAsync(fixtureId, resultDto);
        fixture.Status = FixtureStatus.Completed;
        _context.Fixtures.Update(fixture);

        await AddGoalsAsync(resultDto, fixtureId);

        if (resultDto.MomVote != null)
        {
            await AddMomVoteAsync(resultDto, fixtureId, currentUserId);
        }

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return _mapper.Map<ResultResponseDto>(result);
    }


    private async Task<Fixture> GetFixtureAsync(int fixtureId)
    {
        return await _context.Fixtures.FindAsync(fixtureId)
            ?? throw new KeyNotFoundException("Fixture not found.");
    }

    private string GetCurrentUserId()
    {
        return _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User is not authenticated.");
    }

    private async Task EnsureNoExistingResultAsync(int fixtureId)
    {
        if (await _context.Results.AnyAsync(r => r.FixtureId == fixtureId))
        {
            throw new InvalidOperationException("A result for this fixture has already been submitted.");
        }
    }

    private static void ValidateGoalCount(SubmitResultDto resultDto)
    {
        var totalGoals = resultDto.Goalscorers.Count;
        if (totalGoals != resultDto.HomeScore + resultDto.AwayScore)
        {
            throw new InvalidOperationException("The number of goalscorers does not match the total score.");
        }
    }

    private async Task ValidateGoalscorersAsync(SubmitResultDto resultDto, Fixture fixture)
    {
        var playerIds = resultDto.Goalscorers.Select(g => g.PlayerId).ToList();
        if (!playerIds.Any()) return;

        var validPlayersCount = await _context.Players
            .CountAsync(p => playerIds.Contains(p.Id) &&
                            (p.TeamId == fixture.HomeTeamId || p.TeamId == fixture.AwayTeamId));

        if (validPlayersCount != playerIds.Count)
        {
            throw new InvalidOperationException("One or more goalscorer IDs are invalid or do not belong to the competing teams.");
        }
    }

    private async Task ValidateMomVoteAsync(SubmitResultDto resultDto, Fixture fixture, string currentUserId)
    {
        var userMembership = await _context.TeamMemberships.FirstOrDefaultAsync(m =>
            (m.TeamId == fixture.HomeTeamId || m.TeamId == fixture.AwayTeamId) &&
            m.UserId == currentUserId &&
            (m.Role == TeamRole.Leader || m.Role == TeamRole.AssistantLeader));

        if (userMembership == null)
        {
            throw new UnauthorizedAccessException("User is not a manager of either team in this fixture.");
        }

        var votingTeamId = userMembership.TeamId;
        var opposingTeamId = votingTeamId == fixture.HomeTeamId ? fixture.AwayTeamId : fixture.HomeTeamId;

        var ownPlayerIsValid = await _context.Players.AnyAsync(p =>
            p.Id == resultDto.MomVote!.VotedForOwnPlayerId && p.TeamId == votingTeamId);

        var opponentPlayerIsValid = await _context.Players.AnyAsync(p =>
            p.Id == resultDto.MomVote!.VotedForOpponentPlayerId && p.TeamId == opposingTeamId);

        if (!ownPlayerIsValid || !opponentPlayerIsValid)
        {
            throw new ArgumentException("Invalid player ID in MOM vote. Ensure players belong to the correct teams.");
        }
    }

    private async Task<Result> CreateResultAsync(int fixtureId, SubmitResultDto resultDto)
    {
        var result = new Result
        {
            FixtureId = fixtureId,
            HomeScore = resultDto.HomeScore,
            AwayScore = resultDto.AwayScore,
            Status = ResultStatus.PendingApproval
        };
        _context.Results.Add(result);
        await Task.CompletedTask; // keeps async signature consistent if later expanded
        return result;
    }

    private async Task AddGoalsAsync(SubmitResultDto resultDto, int fixtureId)
    {
        foreach (var goalscorer in resultDto.Goalscorers)
        {
            _context.Goals.Add(new Goal
            {
                PlayerId = goalscorer.PlayerId,
                FixtureId = fixtureId
            });
        }
        await Task.CompletedTask;
    }

    private async Task AddMomVoteAsync(SubmitResultDto resultDto, int fixtureId, string currentUserId)
    {
        var votingTeam = await _context.TeamMemberships.FirstAsync(m => m.UserId == currentUserId);
        var momVote = new MomVote
        {
            FixtureId = fixtureId,
            VotingTeamId = votingTeam.Id,
            VotedForOwnPlayerId = resultDto.MomVote!.VotedForOwnPlayerId,
            VotedForOpponentPlayerId = resultDto.MomVote.VotedForOpponentPlayerId
        };
        _context.MOMVotes.Add(momVote);
    }

}