using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SnakeClassic.Application.Features.BattlePass.Commands.AddXp;
using SnakeClassic.Application.Features.BattlePass.Commands.ClaimReward;
using SnakeClassic.Application.Features.BattlePass.Commands.PurchasePremium;
using SnakeClassic.Application.Features.BattlePass.Queries.GetCurrentSeason;
using SnakeClassic.Application.Features.BattlePass.Queries.GetProgress;

namespace SnakeClassic.Api.Controllers.V1;

public class BattlePassController : BaseApiController
{
    [HttpGet("current-season")]
    [AllowAnonymous]
    public async Task<ActionResult> GetCurrentSeason()
    {
        var result = await Mediator.Send(new GetCurrentSeasonQuery());
        return HandleResult(result);
    }

    [HttpGet("progress")]
    [Authorize]
    public async Task<ActionResult> GetProgress()
    {
        var result = await Mediator.Send(new GetBattlePassProgressQuery());
        return HandleResult(result);
    }

    [HttpPost("add-xp")]
    [Authorize]
    public async Task<ActionResult> AddXp([FromBody] AddXpRequest request)
    {
        var result = await Mediator.Send(new AddBattlePassXpCommand(request.Xp));
        return HandleResult(result);
    }

    [HttpPost("claim-reward")]
    [Authorize]
    public async Task<ActionResult> ClaimReward([FromBody] ClaimBattlePassRewardRequest request)
    {
        var isPremium = request.Tier?.ToLower() == "premium";
        var result = await Mediator.Send(new ClaimBattlePassRewardCommand(request.Level, isPremium));
        return HandleResult(result);
    }

    [HttpPost("purchase-premium")]
    [Authorize]
    public async Task<ActionResult> PurchasePremium()
    {
        var result = await Mediator.Send(new PurchasePremiumCommand());
        return HandleResult(result);
    }
}

public record AddXpRequest(int Xp);
public record ClaimBattlePassRewardRequest(int Level, string? Tier);
