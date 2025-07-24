using LeagueManager.Domain.Models;
using LeagueManager.Application.Dtos;

namespace LeagueManager.Application.Services;

public interface ILocationService
{
    Task<IEnumerable<Location>> GetAllLocationsAsync();
    Task<Location?> GetLocationByIdAsync(int id);
    Task<Location> CreateLocationAsync(LocationDto locationDto);
    Task<Location?> UpdateLocationAsync(int id, LocationDto locationDto);
    Task<bool> DeleteLocationAsync(int id);
}