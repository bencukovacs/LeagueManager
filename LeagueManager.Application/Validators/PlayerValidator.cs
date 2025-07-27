using FluentValidation;
using LeagueManager.Application.Dtos;

namespace LeagueManager.Application.Validators;

public class PlayerDtoValidator : AbstractValidator<PlayerDto>
{
    public PlayerDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Player name is required.")
            .MaximumLength(100).WithMessage("Player name cannot be longer than 100 characters.");
            
        RuleFor(x => x.TeamId)
            .NotEmpty().WithMessage("TeamId is required.");
    }
}