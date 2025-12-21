using MediatR;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Features.Notifications.DTOs;

namespace SnakeClassic.Application.Features.Notifications.Commands.UnsubscribeFromTopic;

public record UnsubscribeFromTopicCommand(string Token, string Topic) : IRequest<Result<SubscribeTopicResultDto>>;
