using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SnakeClassic.Application.Features.Auth.Commands.AuthenticateWithFirebase;
using SnakeClassic.Application.Features.Auth.Queries.GetCurrentUser;

namespace SnakeClassic.Api.Controllers.V1;

public class AuthController : BaseApiController
{
    [HttpPost("firebase")]
    [AllowAnonymous]
    public async Task<ActionResult> AuthenticateWithFirebase([FromBody] AuthenticateRequest request)
    {
        var result = await Mediator.Send(new AuthenticateWithFirebaseCommand(request.IdToken));
        return HandleResult(result);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult> GetCurrentUser()
    {
        var result = await Mediator.Send(new GetCurrentUserQuery());
        return HandleResult(result);
    }

    [HttpPost("logout")]
    [Authorize]
    public ActionResult Logout()
    {
        // JWT tokens are stateless, so logout is typically handled client-side
        return Ok(new { message = "Logged out successfully" });
    }

    [HttpPost("refresh")]
    [Authorize]
    public async Task<ActionResult> RefreshToken()
    {
        // With JWT, refresh is typically done by re-authenticating
        var result = await Mediator.Send(new GetCurrentUserQuery());
        return HandleResult(result);
    }
}

public record AuthenticateRequest(string IdToken);
