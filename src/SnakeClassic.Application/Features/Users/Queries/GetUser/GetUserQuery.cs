using MediatR;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Features.Users.DTOs;

namespace SnakeClassic.Application.Features.Users.Queries.GetUser;

public record GetUserQuery(Guid UserId) : IRequest<Result<UserProfileDto>>;
