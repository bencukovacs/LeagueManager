using LeagueManager.Application.Dtos;

namespace LeagueManager.Application.Services;

public interface ILeagueConfigurationService
{
    Task<LeagueConfigurationDto> GetConfigurationAsync();
    Task<LeagueConfigurationDto> UpdateConfigurationAsync(LeagueConfigurationDto configDto);
}