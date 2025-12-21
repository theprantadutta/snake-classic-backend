using MediatR;
using SnakeClassic.Application.Common;

namespace SnakeClassic.Application.Features.Social.Commands.RejectFriendRequest;

public record RejectFriendRequestCommand(Guid RequestId) : IRequest<Result<bool>>;
