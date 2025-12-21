using MediatR;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Features.Users.DTOs;

namespace SnakeClassic.Application.Features.Users.Queries.SearchUsers;

public record SearchUsersQuery(string Query, int Limit = 20) : IRequest<Result<List<UserSearchResultDto>>>;
