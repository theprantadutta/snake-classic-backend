using MediatR;
using SnakeClassic.Application.Common;

namespace SnakeClassic.Application.Features.Users.Commands.RegisterFcmToken;

public record RegisterFcmTokenCommand(
    string Token,
    string Platform,
    List<string>? SubscribedTopics
) : IRequest<Result<bool>>;
