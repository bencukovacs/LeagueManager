using System.ComponentModel.DataAnnotations;

namespace LeagueManager.API.Dtos;

public class PlayerDto
{
  [Required]
  [StringLength(100)]
  public required string Name { get; set; }

  [Required]
  public int TeamId { get; set; }
}