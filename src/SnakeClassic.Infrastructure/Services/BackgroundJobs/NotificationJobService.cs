using Microsoft.Extensions.Logging;
using SnakeClassic.Application.Common.Interfaces;

namespace SnakeClassic.Infrastructure.Services.BackgroundJobs;

public interface INotificationJobService
{
    Task SendDailyChallengeReminder();
    Task SendWeeklyLeaderboardUpdate();
    Task SendRetentionNotifications();
    Task SendTournamentReminder(string tournamentId, string tournamentName, int minutesUntilStart);
    Task SendTournamentStarted(string tournamentId, string tournamentName);
}

public class NotificationJobService : INotificationJobService
{
    private readonly IFirebaseMessagingService _firebaseMessaging;
    private readonly ILogger<NotificationJobService> _logger;

    public NotificationJobService(
        IFirebaseMessagingService firebaseMessaging,
        ILogger<NotificationJobService> logger)
    {
        _firebaseMessaging = firebaseMessaging;
        _logger = logger;
    }

    public async Task SendDailyChallengeReminder()
    {
        try
        {
            _logger.LogInformation("Sending daily challenge reminders");

            var payload = new NotificationPayload
            {
                Title = "🎯 Daily Challenge Available!",
                Body = "Complete today's challenge to earn bonus rewards!",
                Route = "daily_challenge",
                Data = new Dictionary<string, string>
                {
                    ["action"] = "daily_challenge",
                    ["route"] = "daily_challenge"
                }
            };

            await _firebaseMessaging.SendToTopicAsync(payload, "daily_challenge");

            _logger.LogInformation("Daily challenge reminder sent successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send daily challenge reminder");
        }
    }

    public async Task SendWeeklyLeaderboardUpdate()
    {
        try
        {
            _logger.LogInformation("Sending weekly leaderboard update");

            var payload = new NotificationPayload
            {
                Title = "📊 Weekly Leaderboard Updated!",
                Body = "See how you ranked this week and check out the new challenges!",
                Route = "leaderboard",
                Data = new Dictionary<string, string>
                {
                    ["action"] = "weekly_update",
                    ["route"] = "leaderboard",
                    ["period"] = "weekly"
                }
            };

            await _firebaseMessaging.SendToTopicAsync(payload, "leaderboard_updates");

            _logger.LogInformation("Weekly leaderboard update sent successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send weekly leaderboard update");
        }
    }

    public async Task SendRetentionNotifications()
    {
        try
        {
            _logger.LogInformation("Sending retention notifications");

            var payload = new NotificationPayload
            {
                Title = "🐍 We miss you!",
                Body = "Come back and beat your high score! New achievements await!",
                Route = "home",
                Data = new Dictionary<string, string>
                {
                    ["action"] = "retention",
                    ["route"] = "home",
                    ["campaign"] = "comeback"
                }
            };

            await _firebaseMessaging.SendToTopicAsync(payload, "retention_campaign");

            _logger.LogInformation("Retention notifications sent successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send retention notifications");
        }
    }

    public async Task SendTournamentReminder(string tournamentId, string tournamentName, int minutesUntilStart)
    {
        try
        {
            _logger.LogInformation("Sending tournament reminder for {TournamentName}", tournamentName);

            var payload = new NotificationPayload
            {
                Title = "🏆 Tournament Starting Soon!",
                Body = $"{tournamentName} starts in {minutesUntilStart} minutes!",
                Route = "tournament_detail",
                Data = new Dictionary<string, string>
                {
                    ["action"] = "tournament_reminder",
                    ["route"] = "tournament_detail",
                    ["tournament_id"] = tournamentId,
                    ["minutes_until"] = minutesUntilStart.ToString()
                }
            };

            await _firebaseMessaging.SendToTopicAsync(payload, $"tournament_{tournamentId}");

            _logger.LogInformation("Tournament reminder sent for {TournamentName}", tournamentName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send tournament reminder for {TournamentName}", tournamentName);
        }
    }

    public async Task SendTournamentStarted(string tournamentId, string tournamentName)
    {
        try
        {
            _logger.LogInformation("Sending tournament started notification for {TournamentName}", tournamentName);

            var payload = new NotificationPayload
            {
                Title = "🏆 Tournament Started!",
                Body = $"{tournamentName} has begun! Join now and compete for glory!",
                Route = "tournament_detail",
                Data = new Dictionary<string, string>
                {
                    ["action"] = "tournament_started",
                    ["route"] = "tournament_detail",
                    ["tournament_id"] = tournamentId
                }
            };

            await _firebaseMessaging.SendToTopicAsync(payload, "tournaments");

            _logger.LogInformation("Tournament started notification sent for {TournamentName}", tournamentName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send tournament started notification for {TournamentName}", tournamentName);
        }
    }
}
