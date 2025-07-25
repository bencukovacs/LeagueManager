namespace LeagueManager.Domain.Models;

public enum TeamStatus
{
    PendingApproval,
    Approved,
    Rejected
}

public class Team
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
    public TeamStatus Status { get; set; } = TeamStatus.PendingApproval;
}