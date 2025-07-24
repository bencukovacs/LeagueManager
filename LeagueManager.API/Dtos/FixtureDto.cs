using System.ComponentModel.DataAnnotations;

namespace LeagueManager.API.Dtos;

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