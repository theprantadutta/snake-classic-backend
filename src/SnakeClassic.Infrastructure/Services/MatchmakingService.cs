using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SnakeClassic.Application.Common.Interfaces;
using SnakeClassic.Domain.Entities;
using SnakeClassic.Domain.Enums;

namespace SnakeClassic.Infrastructure.Services;

public interface IMatchmakingService
{
    Task<MatchmakingResult> JoinQueue(Guid userId, string connectionId, MultiplayerGameMode mode, int playerCount);
    Task LeaveQueue(Guid userId);
    Task<List<MatchCreatedResult>> ProcessMatchmaking();
    Task CleanupOldQueueEntries();
}

public class MatchmakingResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public int QueuePosition { get; set; }
    public int EstimatedWaitSeconds { get; set; }
}

public class MatchCreatedResult
{
    public string GameId { get; set; } = null!;
    public string RoomCode { get; set; } = null!;
    public string Mode { get; set; } = null!;
    public int PlayerCount { get; set; }
    public List<MatchedPlayerInfo> Players { get; set; } = new();
}

public class MatchedPlayerInfo
{
    public Guid UserId { get; set; }
    public string? ConnectionId { get; set; }
    public int PlayerIndex { get; set; }
}

public class MatchmakingService : IMatchmakingService
{
    private readonly IAppDbContext _context;
    private readonly ILogger<MatchmakingService> _logger;

    // Player colors for multi-player games
    private static readonly string[] PlayerColors = new[]
    {
        "#4CAF50", "#F44336", "#2196F3", "#FF9800",
        "#9C27B0", "#00BCD4", "#FFEB3B", "#E91E63"
    };

    public MatchmakingService(
        IAppDbContext context,
        ILogger<MatchmakingService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<MatchmakingResult> JoinQueue(Guid userId, string connectionId, MultiplayerGameMode mode, int playerCount)
    {
        try
        {
            // Validate player count
            if (playerCount < 2 || playerCount > 8 || playerCount % 2 != 0)
            {
                return new MatchmakingResult
                {
                    Success = false,
                    Error = "Player count must be 2, 4, 6, or 8"
                };
            }

            // Check if user is already in queue
            var existingEntry = await _context.MatchmakingQueues
                .FirstOrDefaultAsync(q => q.UserId == userId && !q.IsMatched);

            if (existingEntry != null)
            {
                // Update existing entry
                existingEntry.Mode = mode;
                existingEntry.DesiredPlayers = playerCount;
                existingEntry.ConnectionId = connectionId;
                existingEntry.QueuedAt = DateTime.UtcNow;
            }
            else
            {
                // Create new queue entry
                var queueEntry = new MatchmakingQueue
                {
                    UserId = userId,
                    Mode = mode,
                    DesiredPlayers = playerCount,
                    ConnectionId = connectionId,
                    QueuedAt = DateTime.UtcNow,
                    IsMatched = false
                };
                _context.MatchmakingQueues.Add(queueEntry);
            }

            await _context.SaveChangesAsync(default);

            // Calculate queue position
            var queuePosition = await _context.MatchmakingQueues
                .CountAsync(q => q.Mode == mode &&
                    q.DesiredPlayers == playerCount &&
                    !q.IsMatched &&
                    q.QueuedAt < DateTime.UtcNow);

            // Estimate wait time (rough: 10 seconds per position ahead)
            var estimatedWait = queuePosition * 10;

            _logger.LogInformation(
                "User {UserId} joined matchmaking queue for {Mode} {PlayerCount}p. Position: {Position}",
                userId, mode, playerCount, queuePosition + 1);

            return new MatchmakingResult
            {
                Success = true,
                QueuePosition = queuePosition + 1,
                EstimatedWaitSeconds = estimatedWait
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining matchmaking queue for user {UserId}", userId);
            return new MatchmakingResult
            {
                Success = false,
                Error = "Failed to join matchmaking queue"
            };
        }
    }

    public async Task LeaveQueue(Guid userId)
    {
        try
        {
            var entry = await _context.MatchmakingQueues
                .FirstOrDefaultAsync(q => q.UserId == userId && !q.IsMatched);

            if (entry != null)
            {
                _context.MatchmakingQueues.Remove(entry);
                await _context.SaveChangesAsync(default);

                _logger.LogInformation("User {UserId} left matchmaking queue", userId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leaving matchmaking queue for user {UserId}", userId);
        }
    }

    public async Task<List<MatchCreatedResult>> ProcessMatchmaking()
    {
        var createdMatches = new List<MatchCreatedResult>();

        try
        {
            // Get all unmatched queue entries, grouped by mode and player count
            var queueGroups = await _context.MatchmakingQueues
                .Where(q => !q.IsMatched)
                .Include(q => q.User)
                .OrderBy(q => q.QueuedAt)
                .ToListAsync();

            var groupedQueues = queueGroups
                .GroupBy(q => new { q.Mode, q.DesiredPlayers })
                .ToList();

            foreach (var group in groupedQueues)
            {
                var mode = group.Key.Mode;
                var playerCount = group.Key.DesiredPlayers;
                var waitingPlayers = group.ToList();

                // Check if we have enough players
                while (waitingPlayers.Count >= playerCount)
                {
                    // Take the required number of players
                    var matchedPlayers = waitingPlayers.Take(playerCount).ToList();

                    // Create the game
                    var (game, playerInfos) = await CreateMatchedGame(matchedPlayers, mode, playerCount);

                    if (game != null)
                    {
                        // Mark players as matched
                        foreach (var player in matchedPlayers)
                        {
                            player.IsMatched = true;
                            player.MatchedGameId = game.Id;
                        }
                        await _context.SaveChangesAsync(default);

                        // Add to results for notification
                        createdMatches.Add(new MatchCreatedResult
                        {
                            GameId = game.GameId,
                            RoomCode = game.RoomCode,
                            Mode = game.Mode.ToString(),
                            PlayerCount = game.MaxPlayers,
                            Players = playerInfos
                        });

                        // Remove matched players from waiting list
                        waitingPlayers = waitingPlayers.Skip(playerCount).ToList();

                        _logger.LogInformation(
                            "Match created: {GameId} ({Mode} {PlayerCount}p) with room code {RoomCode}",
                            game.GameId, mode, playerCount, game.RoomCode);
                    }
                    else
                    {
                        break; // Failed to create game, skip this group
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing matchmaking");
        }

        return createdMatches;
    }

    public async Task CleanupOldQueueEntries()
    {
        try
        {
            // Clean up old matched entries (older than 5 minutes)
            var cutoff = DateTime.UtcNow.AddMinutes(-5);
            var oldEntries = await _context.MatchmakingQueues
                .Where(q => q.IsMatched && q.QueuedAt < cutoff)
                .ToListAsync();

            if (oldEntries.Count > 0)
            {
                _context.MatchmakingQueues.RemoveRange(oldEntries);
                await _context.SaveChangesAsync(default);
                _logger.LogInformation("Cleaned up {Count} old matchmaking queue entries", oldEntries.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up old queue entries");
        }
    }

    private async Task<(MultiplayerGame? game, List<MatchedPlayerInfo> playerInfos)> CreateMatchedGame(
        List<MatchmakingQueue> players,
        MultiplayerGameMode mode,
        int playerCount)
    {
        try
        {
            // Determine board size based on player count
            var boardSize = playerCount switch
            {
                2 => 20,
                4 => 30,
                _ => 40 // 6-8 players
            };

            // Generate spawn positions
            var spawnPositions = GenerateSpawnPositions(playerCount, boardSize);

            // Generate room code
            var roomCode = GenerateRoomCode();

            // Create game
            var game = new MultiplayerGame
            {
                GameId = Guid.NewGuid().ToString("N")[..8],
                Mode = mode,
                Status = MultiplayerGameStatus.Waiting,
                RoomCode = roomCode,
                MaxPlayers = playerCount,
                HostId = players[0].UserId,
                GameSettings = new Dictionary<string, object>
                {
                    { "boardSize", boardSize },
                    { "gameMode", mode.ToString() }
                },
                CreatedAt = DateTime.UtcNow
            };

            _context.MultiplayerGames.Add(game);
            await _context.SaveChangesAsync(default);

            var playerInfos = new List<MatchedPlayerInfo>();

            // Add players to game
            for (int i = 0; i < players.Count; i++)
            {
                var spawn = spawnPositions[i];
                var multiplayerPlayer = new MultiplayerPlayer
                {
                    GameId = game.Id,
                    UserId = players[i].UserId,
                    PlayerIndex = i,
                    IsReady = true, // Auto-ready for matchmade games
                    SnakeColor = PlayerColors[i],
                    SnakePositions = new List<Dictionary<string, object>>
                    {
                        new() { { "x", spawn.X }, { "y", spawn.Y } },
                        new() { { "x", spawn.X - 1 }, { "y", spawn.Y } },
                        new() { { "x", spawn.X - 2 }, { "y", spawn.Y } }
                    },
                    Direction = spawn.Direction,
                    ConnectionId = players[i].ConnectionId
                };
                _context.MultiplayerPlayers.Add(multiplayerPlayer);

                playerInfos.Add(new MatchedPlayerInfo
                {
                    UserId = players[i].UserId,
                    ConnectionId = players[i].ConnectionId,
                    PlayerIndex = i
                });
            }

            await _context.SaveChangesAsync(default);

            return (game, playerInfos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create matched game");
            return (null, new List<MatchedPlayerInfo>());
        }
    }

    private static List<SpawnPosition> GenerateSpawnPositions(int playerCount, int boardSize)
    {
        var positions = new List<SpawnPosition>();
        var margin = 3; // Distance from edge

        // Distribute players around the board edges
        switch (playerCount)
        {
            case 2:
                // Opposite corners
                positions.Add(new SpawnPosition(margin, margin, "right"));
                positions.Add(new SpawnPosition(boardSize - margin - 1, boardSize - margin - 1, "left"));
                break;
            case 4:
                // Four corners
                positions.Add(new SpawnPosition(margin, margin, "right"));
                positions.Add(new SpawnPosition(boardSize - margin - 1, margin, "left"));
                positions.Add(new SpawnPosition(margin, boardSize - margin - 1, "right"));
                positions.Add(new SpawnPosition(boardSize - margin - 1, boardSize - margin - 1, "left"));
                break;
            case 6:
                // Corners + top/bottom center
                positions.Add(new SpawnPosition(margin, margin, "right"));
                positions.Add(new SpawnPosition(boardSize - margin - 1, margin, "left"));
                positions.Add(new SpawnPosition(boardSize / 2, margin, "down"));
                positions.Add(new SpawnPosition(margin, boardSize - margin - 1, "right"));
                positions.Add(new SpawnPosition(boardSize - margin - 1, boardSize - margin - 1, "left"));
                positions.Add(new SpawnPosition(boardSize / 2, boardSize - margin - 1, "up"));
                break;
            case 8:
                // All edges
                positions.Add(new SpawnPosition(margin, margin, "right"));
                positions.Add(new SpawnPosition(boardSize - margin - 1, margin, "left"));
                positions.Add(new SpawnPosition(boardSize / 2, margin, "down"));
                positions.Add(new SpawnPosition(margin, boardSize / 2, "down"));
                positions.Add(new SpawnPosition(boardSize - margin - 1, boardSize / 2, "up"));
                positions.Add(new SpawnPosition(margin, boardSize - margin - 1, "right"));
                positions.Add(new SpawnPosition(boardSize - margin - 1, boardSize - margin - 1, "left"));
                positions.Add(new SpawnPosition(boardSize / 2, boardSize - margin - 1, "up"));
                break;
        }

        return positions;
    }

    private static string GenerateRoomCode()
    {
        const string chars = "ABCDEFGHJKMNPQRSTUVWXYZ23456789"; // Exclude ambiguous: 0, O, I, L, 1
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 6)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    private class SpawnPosition
    {
        public int X { get; }
        public int Y { get; }
        public string Direction { get; }

        public SpawnPosition(int x, int y, string direction)
        {
            X = x;
            Y = y;
            Direction = direction;
        }
    }
}
