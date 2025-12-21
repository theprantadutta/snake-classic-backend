using MediatR;
using SnakeClassic.Application.Common;

namespace SnakeClassic.Application.Features.Social.Commands.RemoveFriend;

public record RemoveFriendCommand(Guid FriendId) : IRequest<Result<bool>>;
