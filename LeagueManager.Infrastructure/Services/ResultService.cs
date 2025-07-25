using AutoMapper;
using LeagueManager.Application.Dtos;
using LeagueManager.Application.Services;
using LeagueManager.Infrastructure.Data;

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
}