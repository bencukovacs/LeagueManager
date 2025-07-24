namespace LeagueManager.API.Models;

public class Team
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
}