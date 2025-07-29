using LeagueManager.Domain.Models;

namespace LeagueManager.Application.Dtos;

public class SubmitResultDto
{
    public int HomeScore { get; set; }

    public int AwayScore { get; set; }

    public List<GoalscorerDto> Goalscorers { get; set; } = new();

    public MomVoteDto? MomVote { get; set; }
}

public class GoalscorerDto
{
    public int PlayerId { get; set; }
}

public class UpdateResultStatusDto
{
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
    public int VotedForOwnPlayerId { get; set; }

    public int VotedForOpponentPlayerId { get; set; }
}