namespace LeagueManager.Domain.Models;

public enum TeamRole
{
    Leader,
    AssistantLeader,
    Member
}

public class TeamMembership
{
    public int Id { get; set; }
    public required string UserId { get; set; }
    public User? User { get; set; }
    public int TeamId { get; set; }
    public Team? Team { get; set; }
    public TeamRole Role { get; set; }
}