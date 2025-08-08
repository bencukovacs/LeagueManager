namespace LeagueManager.Domain.Models;

public class MomVote
{
    public int Id { get; set; }

    public int FixtureId { get; set; }
    public Fixture? Fixture { get; set; }

    public int VotingTeamId { get; set; }
    public Team? VotingTeam { get; set; }

    public int VotedForOwnPlayerId { get; set; }
    public Player? VotedForOwnPlayer { get; set; }

    public int VotedForOpponentPlayerId { get; set; }
    public Player? VotedForOpponentPlayer { get; set; }
}