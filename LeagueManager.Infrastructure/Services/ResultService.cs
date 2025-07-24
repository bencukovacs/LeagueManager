using LeagueManager.Application.Dtos;
using LeagueManager.Application.Services;
using LeagueManager.Infrastructure.Data;

namespace LeagueManager.Infrastructure.Services;

public class ResultService : IResultService
{
    private readonly LeagueDbContext _context;

    public ResultService(LeagueDbContext context)
    {
        _context = context;
    }

    public async Task<bool> UpdateResultStatusAsync(int resultId, UpdateResultStatusDto statusDto)
    {
        var result = await _context.Results.FindAsync(resultId);
        if (result == null)
        {
            return false;
        }

        result.Status = statusDto.Status;
        await _context.SaveChangesAsync();
        return true;
    }
}
