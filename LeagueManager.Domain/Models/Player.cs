namespace LeagueManager.Domain.Models;

public class Player
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int TeamId { get; set; }
    public Team? Team { get; set; }
    public string? UserId { get; set; }
    public User? User { get; set; }
}