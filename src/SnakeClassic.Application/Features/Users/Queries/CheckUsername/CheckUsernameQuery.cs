using MediatR;
using SnakeClassic.Application.Common;

namespace SnakeClassic.Application.Features.Users.Queries.CheckUsername;

public record CheckUsernameQuery(string Username) : IRequest<Result<CheckUsernameResponse>>;

public record CheckUsernameResponse(bool Available, string? SuggestedUsername);
