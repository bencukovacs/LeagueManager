using System.ComponentModel.DataAnnotations;

namespace LeagueManager.Application.Dtos;

public class CreateFixtureDto
{
  [Required]
  public int HomeTeamId { get; set; }

  [Required]
  public int AwayTeamId { get; set; }

  [Required]
  public DateTime KickOffDateTime { get; set; }

  public int? LocationId { get; set; }
}

public class UpdateFixtureDto
{
  [Required]
  public DateTime KickOffDateTime { get; set; }

  public int? LocationId { get; set; }
}

public class FixtureResponseDto
{
    public int Id { get; set; }
    public required TeamResponseDto HomeTeam { get; set; }
    public required TeamResponseDto AwayTeam { get; set; }
    public DateTime KickOffDateTime { get; set; }
    public required string Status { get; set; }
    public LocationResponseDto? Location { get; set; }
}