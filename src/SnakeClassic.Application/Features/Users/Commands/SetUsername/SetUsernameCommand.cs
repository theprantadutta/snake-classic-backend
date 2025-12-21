using MediatR;
using SnakeClassic.Application.Common;

namespace SnakeClassic.Application.Features.Users.Commands.SetUsername;

public record SetUsernameCommand(string Username) : IRequest<Result<SetUsernameResponse>>;

public record SetUsernameResponse(bool Success, string Username);
