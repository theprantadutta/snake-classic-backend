using MediatR;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Features.Auth.DTOs;

namespace SnakeClassic.Application.Features.Auth.Commands.AuthenticateWithFirebase;

public record AuthenticateWithFirebaseCommand(string IdToken) : IRequest<Result<AuthResponse>>;
