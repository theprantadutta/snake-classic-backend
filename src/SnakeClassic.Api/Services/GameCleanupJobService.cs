using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SnakeClassic.Api.Hubs;
using SnakeClassic.Application.Common.Interfaces;
using SnakeClassic.Domain.Enums;

namespace SnakeClassic.Api.Services;

public interface IGameCleanupJobService
{
    Task CleanupExpiredReconnections();
    Task CleanupAbandonedGames();
    Task ArchiveOldGames();
}

public class GameCleanupJobService : IGameCleanupJobService
{
    private readonly IAppDbContext _context;
    private readonly IHubContext<GameHub> _hubContext;
    private readonly ILogger<GameCleanupJobService> _logger;

    private const int ReconnectionWindowSeconds = 60;
    private const int AbandonedGameMinutes = 30;
    private const int ArchiveAfterHours = 24;

    public GameCleanupJobService(
        IAppDbContext context,
        IHubContext<GameHub> hubContext,
        ILogger<GameCleanupJobService> logger)
    {
        _context = context;
        _hubContext = hubContext;
        _logger = logger;
    }

    /// <summary>
    /// Marks players as permanently dead if their reconnection window (60 seconds) has expired.
    /// Checks if game should end due to only one player remaining.
    /// </summary>
    public async Task CleanupExpiredReconnections()
    {
        try
        {
            var cutoff = DateTime.UtcNow.AddSeconds(-ReconnectionWindowSeconds);

            // Find players who disconnected more than 60 seconds ago and are still marked as alive
            var expiredPlayers = await _context.MultiplayerPlayers
                .Include(p => p.Game)
                .Where(p => p.DisconnectedAt != null &&
                            p.DisconnectedAt < cutoff &&
                            p.IsAlive &&
                            p.Game.Status == MultiplayerGameStatus.Playing)
                .ToListAsync();

            foreach (var player in expiredPlayers)
            {
                // Calculate elimination rank
                var aliveBefore = await _context.MultiplayerPlayers
                    .CountAsync(p => p.GameId == player.GameId && p.IsAlive);

                player.IsAlive = false;
                player.EliminationRank = aliveBefore;
                player.EliminatedAt = DateTime.UtcNow;

                _logger.LogInformation(
                    "Player {UserId} marked as eliminated (rank {Rank}) due to reconnection timeout in game {GameId}",
                    player.UserId, aliveBefore, player.Game.GameId);

                // Notify other players
                await _hubContext.Clients.Group(player.Game.RoomCode)
                    .SendAsync("PlayerEliminated", new
                    {
                        UserId = player.UserId,
                        PlayerIndex = player.PlayerIndex,
                        FinalScore = player.Score,
                        EliminationRank = aliveBefore,
                        Reason = "Disconnection timeout"
                    });
            }

            if (expiredPlayers.Count > 0)
            {
                await _context.SaveChangesAsync(default);
            }

            // Check if any games should end now
            var affectedGameIds = expiredPlayers.Select(p => p.GameId).Distinct().ToList();
            foreach (var gameId in affectedGameIds)
            {
                await CheckAndEndGame(gameId);
            }

            if (expiredPlayers.Count > 0)
            {
                _logger.LogInformation("Cleaned up {Count} expired reconnections", expiredPlayers.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up expired reconnections");
        }
    }

    /// <summary>
    /// Cleans up games that have been in Waiting or Countdown status for too long.
    /// </summary>
    public async Task CleanupAbandonedGames()
    {
        try
        {
            var cutoff = DateTime.UtcNow.AddMinutes(-AbandonedGameMinutes);

            // Find games stuck in Waiting or Countdown for too long
            var abandonedGames = await _context.MultiplayerGames
                .Include(g => g.Players)
                .Where(g => (g.Status == MultiplayerGameStatus.Waiting ||
                             g.Status == MultiplayerGameStatus.Countdown) &&
                            g.CreatedAt < cutoff)
                .ToListAsync();

            foreach (var game in abandonedGames)
            {
                game.Status = MultiplayerGameStatus.Finished;
                game.FinishedAt = DateTime.UtcNow;

                // Notify any connected players
                await _hubContext.Clients.Group(game.RoomCode)
                    .SendAsync("GameCancelled", new
                    {
                        GameId = game.GameId,
                        Reason = "Game abandoned (timeout waiting for players)"
                    });

                _logger.LogInformation("Abandoned game {GameId} marked as finished", game.GameId);
            }

            if (abandonedGames.Count > 0)
            {
                await _context.SaveChangesAsync(default);
                _logger.LogInformation("Cleaned up {Count} abandoned games", abandonedGames.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up abandoned games");
        }
    }

    /// <summary>
    /// Archives/deletes old finished games to keep the database clean.
    /// </summary>
    public async Task ArchiveOldGames()
    {
        try
        {
            var cutoff = DateTime.UtcNow.AddHours(-ArchiveAfterHours);

            // Find old finished games
            var oldGames = await _context.MultiplayerGames
                .Include(g => g.Players)
                .Where(g => g.Status == MultiplayerGameStatus.Finished &&
                            g.FinishedAt < cutoff)
                .ToListAsync();

            foreach (var game in oldGames)
            {
                // Remove players first
                _context.MultiplayerPlayers.RemoveRange(game.Players);
                // Remove game
                _context.MultiplayerGames.Remove(game);
            }

            if (oldGames.Count > 0)
            {
                await _context.SaveChangesAsync(default);
                _logger.LogInformation("Archived {Count} old games", oldGames.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error archiving old games");
        }
    }

    private async Task CheckAndEndGame(Guid gameId)
    {
        try
        {
            var game = await _context.MultiplayerGames
                .Include(g => g.Players)
                .FirstOrDefaultAsync(g => g.Id == gameId);

            if (game == null || game.Status != MultiplayerGameStatus.Playing)
                return;

            var alivePlayers = game.Players.Where(p => p.IsAlive).ToList();

            // End game if only one or zero players alive
            if (alivePlayers.Count <= 1)
            {
                game.Status = MultiplayerGameStatus.Finished;
                game.FinishedAt = DateTime.UtcNow;

                // Winner is the last player standing
                var winner = alivePlayers.FirstOrDefault();
                if (winner != null)
                {
                    winner.EliminationRank = 1;
                    winner.EliminatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync(default);

                // Calculate final results
                var results = game.Players
                    .OrderBy(p => p.EliminationRank ?? int.MaxValue)
                    .ThenByDescending(p => p.Score)
                    .Select((p, index) => new
                    {
                        UserId = p.UserId,
                        PlayerIndex = p.PlayerIndex,
                        Score = p.Score,
                        Rank = p.EliminationRank ?? (index + 1)
                    })
                    .ToList();

                await _hubContext.Clients.Group(game.RoomCode)
                    .SendAsync("GameEnded", new
                    {
                        WinnerId = winner?.UserId,
                        Results = results,
                        FinishedAt = game.FinishedAt,
                        TotalPlayers = game.Players.Count
                    });

                _logger.LogInformation("Game {GameId} ended due to cleanup check", game.GameId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking/ending game {GameId}", gameId);
        }
    }
}
