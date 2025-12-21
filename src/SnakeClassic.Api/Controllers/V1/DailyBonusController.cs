using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SnakeClassic.Application.Features.DailyBonus.Commands.ClaimDailyBonus;
using SnakeClassic.Application.Features.DailyBonus.Queries.GetDailyBonusStatus;

namespace SnakeClassic.Api.Controllers.V1;

[Authorize]
public class DailyBonusController : BaseApiController
{
    [HttpGet("status")]
    public async Task<ActionResult> GetStatus()
    {
        var result = await Mediator.Send(new GetDailyBonusStatusQuery());
        return HandleResult(result);
    }

    [HttpPost("claim")]
    public async Task<ActionResult> Claim()
    {
        var result = await Mediator.Send(new ClaimDailyBonusCommand());
        return HandleResult(result);
    }
}
