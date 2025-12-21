using MediatR;
using Microsoft.EntityFrameworkCore;
using SnakeClassic.Application.Common;
using SnakeClassic.Application.Common.Interfaces;
using SnakeClassic.Application.Features.BattlePass.DTOs;

namespace SnakeClassic.Application.Features.BattlePass.Queries.GetCurrentSeason;

public class GetCurrentSeasonQueryHandler
    : IRequestHandler<GetCurrentSeasonQuery, Result<BattlePassSeasonDto?>>
{
    private readonly IAppDbContext _context;

    public GetCurrentSeasonQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<BattlePassSeasonDto?>> Handle(
        GetCurrentSeasonQuery request,
        CancellationToken cancellationToken)
    {
        var season = await _context.BattlePassSeasons
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.IsActive, cancellationToken);

        if (season == null)
        {
            return Result<BattlePassSeasonDto?>.Success(null);
        }

        return Result<BattlePassSeasonDto?>.Success(new BattlePassSeasonDto(
            season.Id,
            season.SeasonId,
            season.Name,
            season.Description,
            season.StartDate,
            season.EndDate,
            season.MaxLevel,
            season.Price,
            season.IsActive,
            season.LevelsConfig
        ));
    }
}
