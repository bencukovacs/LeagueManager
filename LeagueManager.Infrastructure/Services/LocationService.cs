using LeagueManager.Application.Dtos;
using LeagueManager.Application.Services;
using LeagueManager.Domain.Models;
using LeagueManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LeagueManager.Infrastructure.Services;

public class LocationService : ILocationService
{
    private readonly LeagueDbContext _context;

    public LocationService(LeagueDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Location>> GetAllLocationsAsync()
    {
        return await _context.Locations.ToListAsync();
    }

    public async Task<Location?> GetLocationByIdAsync(int id)
    {
        return await _context.Locations.FindAsync(id);
    }

    public async Task<Location> CreateLocationAsync(LocationDto locationDto)
    {
        var location = new Location
        {
            Name = locationDto.Name,
            Address = locationDto.Address,
            PitchNumber = locationDto.PitchNumber
        };

        _context.Locations.Add(location);
        await _context.SaveChangesAsync();
        return location;
    }

    public async Task<Location?> UpdateLocationAsync(int id, LocationDto locationDto)
    {
        var location = await _context.Locations.FindAsync(id);
        if (location == null)
        {
            return null;
        }

        location.Name = locationDto.Name;
        location.Address = locationDto.Address;
        location.PitchNumber = locationDto.PitchNumber;

        _context.Entry(location).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return location;
    }

    public async Task<bool> DeleteLocationAsync(int id)
    {
        var location = await _context.Locations.FindAsync(id);
        if (location == null)
        {
            return false; // Not found
        }

        var isUsed = await _context.Fixtures.AnyAsync(f => f.LocationId == id);
        if (isUsed)
        {
            throw new InvalidOperationException("Cannot delete location as it is currently assigned to one or more fixtures.");
        }

        _context.Locations.Remove(location);
        await _context.SaveChangesAsync();
        return true; // Success
    }
}