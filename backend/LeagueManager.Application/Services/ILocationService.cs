using LeagueManager.Domain.Models;
using LeagueManager.Application.Dtos;

namespace LeagueManager.Application.Services;

public interface ILocationService
{
    Task<IEnumerable<LocationResponseDto>> GetAllLocationsAsync();
    Task<LocationResponseDto?> GetLocationByIdAsync(int id);
    Task<LocationResponseDto> CreateLocationAsync(LocationDto locationDto);
    Task<LocationResponseDto?> UpdateLocationAsync(int id, LocationDto locationDto);
    Task<bool> DeleteLocationAsync(int id);
}