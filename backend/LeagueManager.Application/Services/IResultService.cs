using LeagueManager.Application.Dtos;

namespace LeagueManager.Application.Services;

public interface IResultService
{
   Task<ResultResponseDto?> UpdateResultStatusAsync(int resultId, UpdateResultStatusDto statusDto);
   Task<IEnumerable<ResultResponseDto>> GetPendingResultsAsync();
}
