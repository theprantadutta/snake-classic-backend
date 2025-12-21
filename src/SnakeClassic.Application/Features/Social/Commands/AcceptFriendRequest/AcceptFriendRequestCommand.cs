using MediatR;
using SnakeClassic.Application.Common;

namespace SnakeClassic.Application.Features.Social.Commands.AcceptFriendRequest;

public record AcceptFriendRequestCommand(Guid RequestId) : IRequest<Result<bool>>;
