using LeagueManager.Domain.Models;
using LeagueManager.Application.Dtos;

namespace LeagueManager.Application.Services;

public interface IFixtureService
{
    Task<IEnumerable<FixtureResponseDto>> GetAllFixturesAsync();
    Task<FixtureResponseDto?> GetFixtureByIdAsync(int id);
    Task<FixtureResponseDto> CreateFixtureAsync(CreateFixtureDto fixtureDto);
    Task<FixtureResponseDto?> UpdateFixtureAsync(int id, UpdateFixtureDto fixtureDto);
    Task<Result> SubmitResultAsync(int fixtureId, SubmitResultDto resultDto);
    Task<bool> DeleteFixtureAsync(int id);
    Task<IEnumerable<MomVoteResponseDto>> GetMomVotesForFixtureAsync(int fixtureId);

}