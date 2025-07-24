using System.ComponentModel.DataAnnotations;
using LeagueManager.API.Models;

namespace LeagueManager.API.Dtos;

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