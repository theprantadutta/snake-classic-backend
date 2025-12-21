using MediatR;
using Microsoft.EntityFrameworkCore;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Common.Interfaces;
using SnakeClassic.Domain.Entities;

namespace SnakeClassic.Application.Features.BattlePass.Commands.PurchasePremium;

public class PurchasePremiumCommandHandler
    : IRequestHandler<PurchasePremiumCommand, Result<PurchasePremiumResultDto>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public PurchasePremiumCommandHandler(IAppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<PurchasePremiumResultDto>> Handle(
        PurchasePremiumCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<PurchasePremiumResultDto>.Unauthorized();
        }

        var season = await _context.BattlePassSeasons
            .FirstOrDefaultAsync(s => s.IsActive, cancellationToken);

        if (season == null)
        {
            return Result<PurchasePremiumResultDto>.Failure("No active battle pass season");
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == _currentUser.UserId.Value, cancellationToken);
        if (user == null)
        {
            return Result<PurchasePremiumResultDto>.NotFound("User not found");
        }

        if (user.Coins < (int)season.Price)
        {
            return Result<PurchasePremiumResultDto>.Failure("Insufficient coins");
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
                HasPremium = true
            };
            _context.UserBattlePassProgresses.Add(progress);
        }
        else if (progress.HasPremium)
        {
            return Result<PurchasePremiumResultDto>.Failure("Already have premium pass");
        }
        else
        {
            progress.HasPremium = true;
        }

        user.Coins -= (int)season.Price;
        await _context.SaveChangesAsync(cancellationToken);

        return Result<PurchasePremiumResultDto>.Success(
            new PurchasePremiumResultDto(true, "Premium battle pass purchased"));
    }
}
