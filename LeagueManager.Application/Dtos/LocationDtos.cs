using System.ComponentModel.DataAnnotations;

namespace LeagueManager.Application.Dtos;

public class LocationDto
{
    [Required]
    [StringLength(100)]
    public required string Name { get; set; }

    [StringLength(200)]
    public string? Address { get; set; }

    [StringLength(50)]
    public string? PitchNumber { get; set; }
}

public class LocationResponseDto
{
     public int Id { get; set; }
    public required string Name { get; set; }
    public string? Address { get; set; }
    public string? PitchNumber { get; set; }
}