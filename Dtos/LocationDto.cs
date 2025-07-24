using System.ComponentModel.DataAnnotations;

namespace LeagueManager.Dtos;

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