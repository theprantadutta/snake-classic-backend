using Microsoft.AspNetCore.SignalR;
using SnakeClassic.Api.Hubs;
using SnakeClassic.Infrastructure.Services;

namespace SnakeClassic.Api.Services;

public interface IMatchmakingJobService
{
    Task ProcessMatchmakingQueue();
}

public class MatchmakingJobService : IMatchmakingJobService
{
    private readonly IMatchmakingService _matchmakingService;
    private readonly IHubContext<GameHub> _hubContext;
    private readonly ILogger<MatchmakingJobService> _logger;

    public MatchmakingJobService(
        IMatchmakingService matchmakingService,
        IHubContext<GameHub> hubContext,
        ILogger<MatchmakingJobService> logger)
    {
        _matchmakingService = matchmakingService;
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task ProcessMatchmakingQueue()
    {
        try
        {
            _logger.LogDebug("Processing matchmaking queue...");

            // Process matchmaking and get created matches
            var createdMatches = await _matchmakingService.ProcessMatchmaking();

            // Notify players via SignalR for each match
            foreach (var match in createdMatches)
            {
                await NotifyMatchFound(match);
            }

            // Cleanup old queue entries
            await _matchmakingService.CleanupOldQueueEntries();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing matchmaking queue");
        }
    }

    private async Task NotifyMatchFound(MatchCreatedResult match)
    {
        try
        {
            foreach (var player in match.Players)
            {
                if (!string.IsNullOrEmpty(player.ConnectionId))
                {
                    await _hubContext.Clients.Client(player.ConnectionId)
                        .SendAsync("MatchFound", new
                        {
                            GameId = match.GameId,
                            RoomCode = match.RoomCode,
                            Mode = match.Mode,
                            PlayerCount = match.PlayerCount,
                            PlayerIndex = player.PlayerIndex
                        });

                    _logger.LogDebug("Notified player {UserId} of match {GameId}",
                        player.UserId, match.GameId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify players of match {GameId}", match.GameId);
        }
    }
}
