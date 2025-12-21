using MediatR;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Features.Social.DTOs;

namespace SnakeClassic.Application.Features.Social.Queries.GetFriends;

public record GetFriendsQuery : IRequest<Result<FriendsResponseDto>>;
