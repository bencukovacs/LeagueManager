using LeagueManager.Domain.Models;
using LeagueManager.Application.Dtos;

namespace LeagueManager.Application.Services;

public interface IFixtureService
{
    Task<IEnumerable<Fixture>> GetAllFixturesAsync();
    Task<Fixture?> GetFixtureByIdAsync(int id);
    Task<Fixture> CreateFixtureAsync(CreateFixtureDto fixtureDto);
    Task<Fixture?> UpdateFixtureAsync(int id, UpdateFixtureDto fixtureDto);
    Task<Result> SubmitResultAsync(int fixtureId, SubmitResultDto resultDto);
    Task<bool> DeleteFixtureAsync(int id);
}