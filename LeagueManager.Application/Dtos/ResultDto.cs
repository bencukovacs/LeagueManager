using System.ComponentModel.DataAnnotations;
using LeagueManager.Domain.Models;

namespace LeagueManager.Application.Dtos;

public class GoalscorerDto
{
    [Required]
    public int PlayerId { get; set; }
}

public class SubmitResultDto
{
    [Required]
    [Range(0, 100)]
    public int HomeScore { get; set; }

    [Required]
    [Range(0, 100)]
    public int AwayScore { get; set; }

    public List<GoalscorerDto> Goalscorers { get; set; } = new();
}

public class UpdateResultStatusDto
{
  [Required]
  public ResultStatus Status { get; set; }
}