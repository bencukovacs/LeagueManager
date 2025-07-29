namespace LeagueManager.Application.Dtos;

public class MomVoteResponseDto
{
    public required string VotingTeamName { get; set; }
    public required string VotedForOwnPlayerName { get; set; }
    public required string VotedForOpponentPlayerName { get; set; }
}