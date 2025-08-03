namespace LeagueManager.Domain.Models;

public class LeagueConfiguration
{
    public int Id { get; set; }
    public int MinPlayersPerTeam { get; set; } = 5; // Default value
    public int MatchLengthMinutes { get; set; } = 25; // Default value
    public int MidSeasonTransferLimit { get; set; } = 2; // Default value
    public DateTime? RosterLockDate { get; set; }
}