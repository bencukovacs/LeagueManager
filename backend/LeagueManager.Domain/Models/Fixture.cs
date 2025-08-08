using System.ComponentModel.DataAnnotations;

namespace LeagueManager.Domain.Models;

public enum FixtureStatus
{
    Scheduled,
    Delayed,
    Completed
}

public class Fixture
{
    public int Id { get; set; }

    public int HomeTeamId { get; set; }
    public Team HomeTeam { get; set; } = default!;

    public int AwayTeamId { get; set; }
    public Team AwayTeam { get; set; } = default!;

    public DateTime KickOffDateTime { get; set; }

    [EnumDataType(typeof(FixtureStatus))]
    public FixtureStatus Status { get; set; } = FixtureStatus.Scheduled;

    public int? LocationId { get; set; }
    public Location? Location { get; set; }

    public Result? Result { get; set; }
}