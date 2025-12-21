using MediatR;
using Microsoft.EntityFrameworkCore;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Common.Interfaces;
using SnakeClassic.Application.Features.Users.DTOs;

namespace SnakeClassic.Application.Features.Users.Queries.GetUser;

public class GetUserQueryHandler : IRequestHandler<GetUserQuery, Result<UserProfileDto>>
{
    private readonly IAppDbContext _context;

    public GetUserQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<UserProfileDto>> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .AsNoTracking()
            .Include(u => u.Preferences)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            return Result<UserProfileDto>.NotFound("User not found");
        }

        var totalGamesPlayed = await _context.Scores
            .CountAsync(s => s.UserId == user.Id, cancellationToken);

        return Result<UserProfileDto>.Success(new UserProfileDto(
            Id: user.Id,
            Email: user.Email,
            Username: user.Username,
            DisplayName: user.DisplayName,
            PhotoUrl: user.PhotoUrl,
            Status: user.Status,
            HighScore: user.HighScore,
            Level: user.Level,
            Coins: user.Coins,
            TotalGamesPlayed: totalGamesPlayed,
            IsAnonymous: user.IsAnonymous,
            CreatedAt: user.CreatedAt,
            LastActiveAt: user.LastActiveAt,
            Preferences: user.Preferences != null ? new UserPreferencesDto(
                Theme: user.Preferences.Theme,
                SoundEnabled: user.Preferences.SoundEnabled,
                MusicEnabled: user.Preferences.MusicEnabled,
                VibrationEnabled: user.Preferences.VibrationEnabled,
                NotificationsEnabled: user.Preferences.NotificationsEnabled
            ) : null
        ));
    }
}
