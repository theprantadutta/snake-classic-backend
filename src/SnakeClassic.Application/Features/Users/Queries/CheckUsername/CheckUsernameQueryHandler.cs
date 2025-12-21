using MediatR;
using Microsoft.EntityFrameworkCore;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Common.Interfaces;

namespace SnakeClassic.Application.Features.Users.Queries.CheckUsername;

public class CheckUsernameQueryHandler : IRequestHandler<CheckUsernameQuery, Result<CheckUsernameResponse>>
{
    private readonly IAppDbContext _context;

    public CheckUsernameQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<CheckUsernameResponse>> Handle(CheckUsernameQuery request, CancellationToken cancellationToken)
    {
        var username = request.Username.Trim().ToLowerInvariant();

        var exists = await _context.Users
            .AnyAsync(u => u.Username != null && u.Username.ToLower() == username, cancellationToken);

        if (!exists)
        {
            return Result<CheckUsernameResponse>.Success(new CheckUsernameResponse(true, null));
        }

        // Generate suggested username
        var random = new Random();
        var suggestedUsername = $"{username}{random.Next(100, 999)}";

        return Result<CheckUsernameResponse>.Success(new CheckUsernameResponse(false, suggestedUsername));
    }
}
