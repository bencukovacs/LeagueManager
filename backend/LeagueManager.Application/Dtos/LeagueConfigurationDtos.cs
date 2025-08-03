namespace LeagueManager.Application.Dtos;

public class LeagueConfigurationDto
{
    public int MinPlayersPerTeam { get; set; }
    public int MatchLengthMinutes { get; set; }
    public int MidSeasonTransferLimit { get; set; }
    public DateTime? RosterLockDate { get; set; }
}