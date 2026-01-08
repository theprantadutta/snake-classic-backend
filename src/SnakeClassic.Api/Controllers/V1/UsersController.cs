using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SnakeClassic.Application.Features.Auth.Queries.GetCurrentUser;
using SnakeClassic.Application.Features.Users.Commands.RegisterFcmToken;
using SnakeClassic.Application.Features.Users.Commands.ResetStatistics;
using SnakeClassic.Application.Features.Users.Commands.SetUsername;
using SnakeClassic.Application.Features.Users.Commands.UpdateProfile;
using SnakeClassic.Application.Features.Users.Queries.CheckUsername;
using SnakeClassic.Application.Features.Users.Queries.GetUser;
using SnakeClassic.Application.Features.Users.Queries.SearchUsers;

namespace SnakeClassic.Api.Controllers.V1;

[Authorize]
public class UsersController : BaseApiController
{
    [HttpGet("me")]
    public async Task<ActionResult> GetMe()
    {
        var result = await Mediator.Send(new GetCurrentUserQuery());
        return HandleResult(result);
    }

    [HttpPut("me")]
    public async Task<ActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var command = new UpdateProfileCommand(
            request.DisplayName,
            request.PhotoUrl,
            request.Preferences != null ? new UserPreferencesUpdateDto(
                request.Preferences.Theme,
                request.Preferences.SoundEnabled,
                request.Preferences.MusicEnabled,
                request.Preferences.VibrationEnabled,
                request.Preferences.NotificationsEnabled
            ) : null
        );
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpPost("username/check")]
    public async Task<ActionResult> CheckUsername([FromBody] CheckUsernameRequest request)
    {
        var result = await Mediator.Send(new CheckUsernameQuery(request.Username));
        return HandleResult(result);
    }

    [HttpPut("username")]
    public async Task<ActionResult> SetUsername([FromBody] SetUsernameRequest request)
    {
        var result = await Mediator.Send(new SetUsernameCommand(request.Username));
        return HandleResult(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult> GetUser(Guid id)
    {
        var result = await Mediator.Send(new GetUserQuery(id));
        return HandleResult(result);
    }

    [HttpGet("search")]
    public async Task<ActionResult> SearchUsers([FromQuery] string query, [FromQuery] int limit = 20)
    {
        var result = await Mediator.Send(new SearchUsersQuery(query, limit));
        return HandleResult(result);
    }

    [HttpPost("register-token")]
    public async Task<ActionResult> RegisterFcmToken([FromBody] RegisterTokenRequest request)
    {
        var command = new RegisterFcmTokenCommand(
            request.FcmToken,
            request.Platform,
            request.SubscribedTopics
        );
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpPost("me/reset-statistics")]
    public async Task<ActionResult> ResetStatistics()
    {
        var result = await Mediator.Send(new ResetStatisticsCommand());
        return HandleResult(result);
    }
}

public record UpdateProfileRequest(
    string? DisplayName,
    string? PhotoUrl,
    PreferencesRequest? Preferences
);

public record PreferencesRequest(
    string? Theme,
    bool? SoundEnabled,
    bool? MusicEnabled,
    bool? VibrationEnabled,
    bool? NotificationsEnabled
);

public record CheckUsernameRequest(string Username);
public record SetUsernameRequest(string Username);
public record RegisterTokenRequest(string FcmToken, string Platform, List<string>? SubscribedTopics);
