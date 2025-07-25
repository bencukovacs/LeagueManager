using System.ComponentModel.DataAnnotations;

namespace LeagueManager.Application.Dtos;

public class PlayerDto
{
  [Required]
  [StringLength(100)]
  public required string Name { get; set; }

  [Required]
  public int TeamId { get; set; }
}

public class PlayerResponseDto
{
  public int Id { get; set; }
  public required string Name { get; set; }
  public string? TeamName { get; set; }
}
