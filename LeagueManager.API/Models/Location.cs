namespace LeagueManager.API.Models;

public class Location
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Address { get; set; }
    public string? PitchNumber { get; set; }
}