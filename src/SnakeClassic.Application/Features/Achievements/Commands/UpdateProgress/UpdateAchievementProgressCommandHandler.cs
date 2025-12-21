using MediatR;
using Microsoft.EntityFrameworkCore;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Common.Interfaces;
using SnakeClassic.Application.Features.Achievements.DTOs;
using SnakeClassic.Domain.Entities;

namespace SnakeClassic.Application.Features.Achievements.Commands.UpdateProgress;

public class UpdateAchievementProgressCommandHandler
    : IRequestHandler<UpdateAchievementProgressCommand, Result<UpdateProgressResultDto>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeService _dateTime;

    public UpdateAchievementProgressCommandHandler(
        IAppDbContext context,
        ICurrentUserService currentUser,
        IDateTimeService dateTime)
    {
        _context = context;
        _currentUser = currentUser;
        _dateTime = dateTime;
    }

    public async Task<Result<UpdateProgressResultDto>> Handle(
        UpdateAchievementProgressCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<UpdateProgressResultDto>.Unauthorized();
        }

        var achievement = await _context.Achievements
            .FirstOrDefaultAsync(a => a.AchievementId == request.AchievementId, cancellationToken);

        if (achievement == null)
        {
            return Result<UpdateProgressResultDto>.NotFound("Achievement not found");
        }

        var userAchievement = await _context.UserAchievements
            .FirstOrDefaultAsync(ua =>
                ua.UserId == _currentUser.UserId.Value &&
                ua.AchievementId == achievement.Id, cancellationToken);

        bool newlyUnlocked = false;

        if (userAchievement == null)
        {
            var newProgress = Math.Min(request.ProgressIncrement, achievement.RequirementValue);
            userAchievement = new UserAchievement
            {
                UserId = _currentUser.UserId.Value,
                AchievementId = achievement.Id,
                CurrentProgress = newProgress
            };

            if (newProgress >= achievement.RequirementValue)
            {
                userAchievement.IsUnlocked = true;
                userAchievement.UnlockedAt = _dateTime.UtcNow;
                newlyUnlocked = true;
            }

            _context.UserAchievements.Add(userAchievement);
        }
        else if (!userAchievement.IsUnlocked)
        {
            // Increment the progress
            userAchievement.CurrentProgress = Math.Min(
                userAchievement.CurrentProgress + request.ProgressIncrement,
                achievement.RequirementValue);

            if (userAchievement.CurrentProgress >= achievement.RequirementValue)
            {
                userAchievement.IsUnlocked = true;
                userAchievement.UnlockedAt = _dateTime.UtcNow;
                newlyUnlocked = true;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<UpdateProgressResultDto>.Success(new UpdateProgressResultDto(
            Success: true,
            NewlyUnlocked: newlyUnlocked,
            CurrentProgress: userAchievement.CurrentProgress,
            RequiredProgress: achievement.RequirementValue
        ));
    }
}
