using FluentValidation;

namespace SnakeClassic.Application.Features.Scores.Commands.SubmitScore;

public class SubmitScoreCommandValidator : AbstractValidator<SubmitScoreCommand>
{
    public SubmitScoreCommandValidator()
    {
        RuleFor(x => x.Score)
            .GreaterThanOrEqualTo(0).WithMessage("Score must be non-negative");

        RuleFor(x => x.GameDurationSeconds)
            .GreaterThanOrEqualTo(0).WithMessage("Game duration must be non-negative");

        RuleFor(x => x.FoodsEaten)
            .GreaterThanOrEqualTo(0).WithMessage("Foods eaten must be non-negative");

        RuleFor(x => x.GameMode)
            .NotEmpty().WithMessage("Game mode is required");

        RuleFor(x => x.Difficulty)
            .NotEmpty().WithMessage("Difficulty is required");
    }
}
