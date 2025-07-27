using AutoMapper;
using AutoMapper.QueryableExtensions;
using LeagueManager.Application.Dtos;
using LeagueManager.Application.Services;
using LeagueManager.Domain.Models;
using LeagueManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LeagueManager.Infrastructure.Services;

public class ResultService : IResultService
{
    private readonly LeagueDbContext _context;
    private readonly IMapper _mapper;

    public ResultService(LeagueDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ResultResponseDto?> UpdateResultStatusAsync(int resultId, UpdateResultStatusDto statusDto)
    {
        var result = await _context.Results.FindAsync(resultId);

        if (result == null)
        {
            return null;
        }

        result.Status = statusDto.Status;
        await _context.SaveChangesAsync();

        return _mapper.Map<ResultResponseDto>(result);
    }
    public async Task<IEnumerable<ResultResponseDto>> GetPendingResultsAsync()
    {
        return await _context.Results
            .Where(r => r.Status == ResultStatus.PendingApproval)
            .ProjectTo<ResultResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }
}