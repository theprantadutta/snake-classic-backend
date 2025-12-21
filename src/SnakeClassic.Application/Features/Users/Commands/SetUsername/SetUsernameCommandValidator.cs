using FluentValidation;

namespace SnakeClassic.Application.Features.Users.Commands.SetUsername;

public class SetUsernameCommandValidator : AbstractValidator<SetUsernameCommand>
{
    public SetUsernameCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters")
            .MaximumLength(20).WithMessage("Username must not exceed 20 characters")
            .Matches(@"^[a-zA-Z0-9_]+$").WithMessage("Username can only contain letters, numbers, and underscores");
    }
}
