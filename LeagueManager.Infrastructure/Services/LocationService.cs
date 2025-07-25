using AutoMapper;
using LeagueManager.Application.Dtos;
using LeagueManager.Application.Services;
using LeagueManager.Domain.Models;
using LeagueManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LeagueManager.Infrastructure.Services;

public class LocationService : ILocationService
{
    private readonly LeagueDbContext _context;
    private readonly IMapper _mapper;

    public LocationService(LeagueDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<LocationResponseDto>> GetAllLocationsAsync()
    {
        var locations = await _context.Locations.ToListAsync();
        return _mapper.Map<IEnumerable<LocationResponseDto>>(locations);
    }

    public async Task<LocationResponseDto?> GetLocationByIdAsync(int id)
    {
        var location = await _context.Locations.FindAsync(id);
        return _mapper.Map<LocationResponseDto>(location);
    }

    public async Task<LocationResponseDto> CreateLocationAsync(LocationDto locationDto)
    {
        var location = _mapper.Map<Location>(locationDto);

        _context.Locations.Add(location);
        await _context.SaveChangesAsync();
        return _mapper.Map<LocationResponseDto>(location);
    }

    public async Task<LocationResponseDto?> UpdateLocationAsync(int id, LocationDto locationDto)
    {
        var location = await _context.Locations.FindAsync(id);
        if (location == null)
        {
            return null;
        }

        _mapper.Map(locationDto, location);
        
        await _context.SaveChangesAsync();
        return _mapper.Map<LocationResponseDto>(location);
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