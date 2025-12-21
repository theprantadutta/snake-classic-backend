using MediatR;
using SnakeClassic.Application.Common;

namespace SnakeClassic.Application.Features.Social.Commands.SendFriendRequest;

public record SendFriendRequestCommand(Guid FriendId) : IRequest<Result<SendFriendRequestResultDto>>;

public record SendFriendRequestResultDto(bool Success, string Message);
