using MediatR;
using Microsoft.EntityFrameworkCore;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Common.Interfaces;
using SnakeClassic.Application.Features.Users.DTOs;

namespace SnakeClassic.Application.Features.Users.Queries.SearchUsers;

public class SearchUsersQueryHandler : IRequestHandler<SearchUsersQuery, Result<List<UserSearchResultDto>>>
{
    private readonly IAppDbContext _context;

    public SearchUsersQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<UserSearchResultDto>>> Handle(SearchUsersQuery request, CancellationToken cancellationToken)
    {
        var query = request.Query.Trim().ToLowerInvariant();

        var users = await _context.Users
            .AsNoTracking()
            .Where(u => (u.Username != null && u.Username.ToLower().Contains(query)) ||
                        (u.DisplayName != null && u.DisplayName.ToLower().Contains(query)))
            .Take(request.Limit)
            .Select(u => new UserSearchResultDto(
                u.Id,
                u.Username,
                u.DisplayName,
                u.PhotoUrl,
                u.HighScore,
                u.Level
            ))
            .ToListAsync(cancellationToken);

        return Result<List<UserSearchResultDto>>.Success(users);
    }
}
