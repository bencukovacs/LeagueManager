namespace LeagueManager.Models;

public class Team
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string? PrimaryColor { get; set; }
    public required string? SecondaryColor { get; set; }
}