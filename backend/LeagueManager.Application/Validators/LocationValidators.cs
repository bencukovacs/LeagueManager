using FluentValidation;
using LeagueManager.Application.Dtos;

namespace LeagueManager.Application.Validators;

public class LocationDtoValidator : AbstractValidator<LocationDto>
{
    public LocationDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Location name is required.")
            .MaximumLength(100).WithMessage("Location name cannot be longer than 100 characters.");

        RuleFor(x => x.Address)
            .MaximumLength(200).WithMessage("Address cannot be longer than 200 characters.");
            
        RuleFor(x => x.PitchNumber)
            .MaximumLength(50).WithMessage("Pitch number cannot be longer than 50 characters.");
    }
}