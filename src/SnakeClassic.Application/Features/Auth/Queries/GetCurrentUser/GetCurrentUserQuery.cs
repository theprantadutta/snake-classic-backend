using MediatR;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Features.Auth.DTOs;

namespace SnakeClassic.Application.Features.Auth.Queries.GetCurrentUser;

public record GetCurrentUserQuery : IRequest<Result<UserDto>>;
