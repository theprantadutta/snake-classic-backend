using MediatR;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Features.Social.DTOs;

namespace SnakeClassic.Application.Features.Social.Queries.GetFriendRequests;

public record GetFriendRequestsQuery : IRequest<Result<List<FriendRequestDto>>>;
