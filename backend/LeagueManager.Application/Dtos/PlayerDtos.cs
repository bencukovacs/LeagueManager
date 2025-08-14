using System.ComponentModel.DataAnnotations;

namespace LeagueManager.Application.Dtos;

public class PlayerDto
{
  public required string Name { get; set; }

  public int TeamId { get; set; }
}

public class PlayerResponseDto
{
  public int Id { get; set; }
  public required string Name { get; set; }
  public int? TeamId { get; set; }
  public string? TeamName { get; set; }
  public string? UserId { get; set; }
  public string? UserRole { get; set; } 
}
