using System.ComponentModel.DataAnnotations;

namespace LeagueManager.Domain.Models;

public enum ResultStatus
{
    PendingApproval,
    Approved,
    Disputed
}

public class Result
{
    public int Id { get; set; }

    public int FixtureId { get; set; }
    public Fixture? Fixture { get; set; }

    public int HomeScore { get; set; }
    public int AwayScore { get; set; }

    [EnumDataType(typeof(ResultStatus))]
    public ResultStatus Status { get; set; } = ResultStatus.PendingApproval;
}