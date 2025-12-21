using MediatR;
using Microsoft.EntityFrameworkCore;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Common.Interfaces;
using SnakeClassic.Application.Features.BattlePass.DTOs;
using SnakeClassic.Domain.Entities;

namespace SnakeClassic.Application.Features.BattlePass.Commands.AddXp;

public class AddBattlePassXpCommandHandler
    : IRequestHandler<AddBattlePassXpCommand, Result<AddXpResultDto>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public AddBattlePassXpCommandHandler(IAppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<AddXpResultDto>> Handle(
        AddBattlePassXpCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<AddXpResultDto>.Unauthorized();
        }

        var season = await _context.BattlePassSeasons
            .FirstOrDefaultAsync(s => s.IsActive, cancellationToken);

        if (season == null)
        {
            return Result<AddXpResultDto>.Failure("No active battle pass season");
        }

        var progress = await _context.UserBattlePassProgresses
            .FirstOrDefaultAsync(p =>
                p.UserId == _currentUser.UserId.Value &&
                p.SeasonId == season.Id, cancellationToken);

        if (progress == null)
        {
            progress = new UserBattlePassProgress
            {
                UserId = _currentUser.UserId.Value,
                SeasonId = season.Id,
                CurrentLevel = 0,
                CurrentXp = 0
            };
            _context.UserBattlePassProgresses.Add(progress);
        }

        var startLevel = progress.CurrentLevel;
        progress.CurrentXp += request.XpAmount;

        // Calculate level ups
        var levelsGained = 0;
        while (progress.CurrentLevel < season.MaxLevel)
        {
            var xpNeeded = 100 + (progress.CurrentLevel * 50);
            if (progress.CurrentXp >= xpNeeded)
            {
                progress.CurrentXp -= xpNeeded;
                progress.CurrentLevel++;
                levelsGained++;
            }
            else
            {
                break;
            }
        }

        // Cap XP if at max level
        if (progress.CurrentLevel >= season.MaxLevel)
        {
            progress.CurrentLevel = season.MaxLevel;
            progress.CurrentXp = 0;
        }

        await _context.SaveChangesAsync(cancellationToken);

        var xpToNextLevel = progress.CurrentLevel < season.MaxLevel
            ? 100 + (progress.CurrentLevel * 50)
            : 0;

        return Result<AddXpResultDto>.Success(new AddXpResultDto(
            NewLevel: progress.CurrentLevel,
            NewXp: progress.CurrentXp,
            XpToNextLevel: xpToNextLevel,
            LeveledUp: levelsGained > 0,
            LevelsGained: levelsGained
        ));
    }
}
