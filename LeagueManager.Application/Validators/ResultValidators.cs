using FluentValidation;
using LeagueManager.Application.Dtos;

namespace LeagueManager.Application.Validators;

public class SubmitResultDtoValidator : AbstractValidator<SubmitResultDto>
{
    public SubmitResultDtoValidator()
    {
        RuleFor(x => x.HomeScore).InclusiveBetween(0, 100);
        RuleFor(x => x.AwayScore).InclusiveBetween(0, 100);

        // This rule ensures that if MomVote is provided, its properties are also validated
        RuleFor(x => x.MomVote).SetValidator(new MomVoteDtoValidator()!); 
    }
}

public class MomVoteDtoValidator : AbstractValidator<MomVoteDto>
{
  public MomVoteDtoValidator()
  {
    RuleFor(x => x.VotedForOwnPlayerId).NotEmpty();
    RuleFor(x => x.VotedForOpponentPlayerId).NotEmpty();
  }
}

public class GoalscorerDtoValidator : AbstractValidator<GoalscorerDto>
{
    public GoalscorerDtoValidator()
    {
        RuleFor(x => x.PlayerId)
            .NotEmpty().WithMessage("PlayerId is required for each goalscorer.");
    }
}

public class UpdateResultStatusDtoValidator : AbstractValidator<UpdateResultStatusDto>
{
    public UpdateResultStatusDtoValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("A valid result status is required.");
    }
}