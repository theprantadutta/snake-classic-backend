using MediatR;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Features.Notifications.DTOs;

namespace SnakeClassic.Application.Features.Notifications.Commands.SubscribeToTopic;

public record SubscribeToTopicCommand(string Token, string Topic) : IRequest<Result<SubscribeTopicResultDto>>;
