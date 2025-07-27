using FluentValidation;
using LeagueManager.Application.Dtos;

namespace LeagueManager.Application.Validators;

public class CreateTeamDtoValidator : AbstractValidator<CreateTeamDto>
{
    public CreateTeamDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Team name is required.")
            .MaximumLength(100).WithMessage("Team name cannot be longer than 100 characters.");

        RuleFor(x => x.PrimaryColor)
            .MaximumLength(50).WithMessage("Primary color cannot be longer than 50 characters.");
            
        RuleFor(x => x.SecondaryColor)
            .MaximumLength(50).WithMessage("Secondary color cannot be longer than 50 characters.");
    }
}