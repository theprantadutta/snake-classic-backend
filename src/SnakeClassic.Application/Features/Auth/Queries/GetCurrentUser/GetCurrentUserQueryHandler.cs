using MediatR;
using Microsoft.EntityFrameworkCore;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Common.Interfaces;
using SnakeClassic.Application.Features.Auth.DTOs;

namespace SnakeClassic.Application.Features.Auth.Queries.GetCurrentUser;

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, Result<UserDto>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetCurrentUserQueryHandler(IAppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<UserDto>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<UserDto>.Unauthorized();
        }

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == _currentUser.UserId.Value, cancellationToken);

        if (user == null)
        {
            return Result<UserDto>.NotFound("User not found");
        }

        return Result<UserDto>.Success(new UserDto(
            Id: user.Id,
            Email: user.Email,
            Username: user.Username,
            DisplayName: user.DisplayName,
            PhotoUrl: user.PhotoUrl,
            Status: user.Status,
            HighScore: user.HighScore,
            Level: user.Level,
            Coins: user.Coins,
            IsAnonymous: user.IsAnonymous,
            CreatedAt: user.CreatedAt,
            LastActiveAt: user.LastActiveAt
        ));
    }
}
