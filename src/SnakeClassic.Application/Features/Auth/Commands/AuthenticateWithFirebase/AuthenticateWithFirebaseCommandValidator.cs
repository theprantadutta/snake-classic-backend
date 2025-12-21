using FluentValidation;

namespace SnakeClassic.Application.Features.Auth.Commands.AuthenticateWithFirebase;

public class AuthenticateWithFirebaseCommandValidator : AbstractValidator<AuthenticateWithFirebaseCommand>
{
    public AuthenticateWithFirebaseCommandValidator()
    {
        RuleFor(x => x.IdToken)
            .NotEmpty().WithMessage("Firebase ID token is required");
    }
}
