using System.ComponentModel.DataAnnotations;

namespace LeagueManager.Application.Dtos;

public class CreateFixtureDto
{
  public int HomeTeamId { get; set; }

  public int AwayTeamId { get; set; }

  public DateTime KickOffDateTime { get; set; }

  public int? LocationId { get; set; }
}

public class UpdateFixtureDto
{
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
  public ResultResponseDto? Result { get; set; }
  public List<PlayerResponseDto> HomeTeamRoster { get; set; } = new();
  public List<PlayerResponseDto> AwayTeamRoster { get; set; } = new();
}