using MediatR;
using Microsoft.EntityFrameworkCore;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Common.Interfaces;
using SnakeClassic.Application.Features.Users.DTOs;

namespace SnakeClassic.Application.Features.Users.Commands.UpdateProfile;

public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, Result<UserProfileDto>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateProfileCommandHandler(IAppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<UserProfileDto>> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<UserProfileDto>.Unauthorized();
        }

        var user = await _context.Users
            .Include(u => u.Preferences)
            .FirstOrDefaultAsync(u => u.Id == _currentUser.UserId.Value, cancellationToken);

        if (user == null)
        {
            return Result<UserProfileDto>.NotFound("User not found");
        }

        // Update user fields
        if (!string.IsNullOrEmpty(request.DisplayName))
            user.DisplayName = request.DisplayName;
        if (!string.IsNullOrEmpty(request.PhotoUrl))
            user.PhotoUrl = request.PhotoUrl;

        // Update preferences if provided
        if (request.Preferences != null && user.Preferences != null)
        {
            if (!string.IsNullOrEmpty(request.Preferences.Theme))
                user.Preferences.Theme = request.Preferences.Theme;
            if (request.Preferences.SoundEnabled.HasValue)
                user.Preferences.SoundEnabled = request.Preferences.SoundEnabled.Value;
            if (request.Preferences.MusicEnabled.HasValue)
                user.Preferences.MusicEnabled = request.Preferences.MusicEnabled.Value;
            if (request.Preferences.VibrationEnabled.HasValue)
                user.Preferences.VibrationEnabled = request.Preferences.VibrationEnabled.Value;
            if (request.Preferences.NotificationsEnabled.HasValue)
                user.Preferences.NotificationsEnabled = request.Preferences.NotificationsEnabled.Value;
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Get total games count
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
