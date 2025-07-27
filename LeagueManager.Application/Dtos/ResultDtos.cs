using LeagueManager.Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace LeagueManager.Application.Dtos;

public class SubmitResultDto
{
    [Required]
    [Range(0, 100)]
    public int HomeScore { get; set; }

    [Required]
    [Range(0, 100)]
    public int AwayScore { get; set; }

    public List<GoalscorerDto> Goalscorers { get; set; } = new();
    
    public MomVoteDto? MomVote { get; set; }
}

public class GoalscorerDto
{
    [Required]
    public int PlayerId { get; set; }
}

public class UpdateResultStatusDto
{
    [Required]
    public ResultStatus Status { get; set; }
}

public class ResultResponseDto
{
    public int Id { get; set; }
    public int FixtureId { get; set; }
    public int HomeScore { get; set; }
    public int AwayScore { get; set; }
    public required string Status { get; set; }
}

public class MomVoteDto
{
    [Required]
    public int VotedForOwnPlayerId { get; set; }

    [Required]
    public int VotedForOpponentPlayerId { get; set; }
}