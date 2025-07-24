using LeagueManager.Application.Dtos;

namespace LeagueManager.Application.Services;

public interface IResultService
{
    Task<bool> UpdateResultStatusAsync(int resultId, UpdateResultStatusDto statusDto);
}
