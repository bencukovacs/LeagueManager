using FluentValidation;
using LeagueManager.Application.Dtos;

namespace LeagueManager.Application.Validators;

public class CreateFixtureDtoValidator : AbstractValidator<CreateFixtureDto>
{
    public CreateFixtureDtoValidator()
    {
        RuleFor(x => x.HomeTeamId).NotEmpty();
        RuleFor(x => x.AwayTeamId).NotEmpty();
        RuleFor(x => x.KickOffDateTime).NotEmpty();
    }
}

public class UpdateFixtureDtoValidator : AbstractValidator<UpdateFixtureDto>
{
    public UpdateFixtureDtoValidator()
    {
        RuleFor(x => x.KickOffDateTime).NotEmpty();
    }
}