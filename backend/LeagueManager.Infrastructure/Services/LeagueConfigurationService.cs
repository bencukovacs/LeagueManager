using AutoMapper;
using LeagueManager.Application.Dtos;
using LeagueManager.Application.Services;
using LeagueManager.Domain.Models;
using LeagueManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LeagueManager.Infrastructure.Services;

public class LeagueConfigurationService : ILeagueConfigurationService
{
    private readonly LeagueDbContext _context;
    private readonly IMapper _mapper;

    public LeagueConfigurationService(LeagueDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<LeagueConfigurationDto> GetConfigurationAsync()
    {
        // For now, we assume there is only one configuration record.
        // If it doesn't exist, we create it with default values.
        var config = await _context.LeagueConfigurations.FirstOrDefaultAsync();
        if (config == null)
        {
            config = new LeagueConfiguration();
            _context.LeagueConfigurations.Add(config);
            await _context.SaveChangesAsync();
        }
        return _mapper.Map<LeagueConfigurationDto>(config);
    }

    public async Task<LeagueConfigurationDto> UpdateConfigurationAsync(LeagueConfigurationDto configDto)
    {
        var config = await _context.LeagueConfigurations.FirstOrDefaultAsync() 
            ?? throw new InvalidOperationException("Configuration not found.");

        _mapper.Map(configDto, config);
        await _context.SaveChangesAsync();
        return _mapper.Map<LeagueConfigurationDto>(config);
    }
}