using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SnakeClassic.Application.Common.Interfaces;
using SnakeClassic.Domain.Enums;
using SnakeClassic.Infrastructure.Services;

namespace SnakeClassic.Api.Hubs;

[Authorize]
public class GameHub : Hub
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IMatchmakingService _matchmakingService;
    private readonly ILogger<GameHub> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IHubContext<GameHub> _hubContext;

    public GameHub(
        IAppDbContext context,
        ICurrentUserService currentUser,
        IMatchmakingService matchmakingService,
        ILogger<GameHub> logger,
        IServiceScopeFactory serviceScopeFactory,
        IHubContext<GameHub> hubContext)
    {
        _context = context;
        _currentUser = currentUser;
        _matchmakingService = matchmakingService;
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _hubContext = hubContext;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("User {UserId} connected to GameHub", _currentUser.UserId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("User {UserId} disconnected from GameHub", _currentUser.UserId);

        if (_currentUser.UserId.HasValue)
        {
            // Remove from matchmaking queue if in one
            await _matchmakingService.LeaveQueue(_currentUser.UserId.Value);

            // Handle player disconnection - don't leave game, just mark as disconnected for reconnection
            var player = await _context.MultiplayerPlayers
                .Include(p => p.Game)
                .FirstOrDefaultAsync(p => p.UserId == _currentUser.UserId.Value &&
                    p.Game.Status != MultiplayerGameStatus.Finished &&
                    p.IsAlive);

            if (player != null)
            {
                var roomCode = player.Game.RoomCode;

                // If game is in Waiting status, remove player entirely
                if (player.Game.Status == MultiplayerGameStatus.Waiting)
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomCode);
                    _context.MultiplayerPlayers.Remove(player);
                    await _context.SaveChangesAsync(default);

                    await Clients.Group(roomCode).SendAsync("PlayerLeft", new
                    {
                        UserId = _currentUser.UserId.Value,
                        PlayerIndex = player.PlayerIndex
                    });

                    _logger.LogInformation("User {UserId} left waiting room {RoomCode}", _currentUser.UserId, roomCode);
                }
                // If game is Playing, mark as disconnected (allow reconnection)
                else if (player.Game.Status == MultiplayerGameStatus.Playing)
                {
                    player.DisconnectedAt = DateTime.UtcNow;
                    player.ConnectionId = null;
                    await _context.SaveChangesAsync(default);

                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomCode);

                    // Notify others of temporary disconnection
                    await Clients.Group(roomCode).SendAsync("PlayerDisconnected", new
                    {
                        UserId = _currentUser.UserId.Value,
                        PlayerIndex = player.PlayerIndex,
                        CanReconnect = true,
                        ReconnectWindowSeconds = 60
                    });

                    _logger.LogInformation("User {UserId} disconnected from active game {RoomCode}, can reconnect within 60s",
                        _currentUser.UserId, roomCode);
                }
            }
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinRoom(string roomCode)
    {
        if (!_currentUser.UserId.HasValue)
        {
            await Clients.Caller.SendAsync("Error", "User not authenticated");
            return;
        }

        var game = await _context.MultiplayerGames
            .Include(g => g.Players)
            .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(g => g.RoomCode == roomCode);

        if (game == null)
        {
            await Clients.Caller.SendAsync("Error", "Game not found");
            return;
        }

        if (game.Status != MultiplayerGameStatus.Waiting)
        {
            await Clients.Caller.SendAsync("Error", "Game already in progress");
            return;
        }

        var existingPlayer = game.Players.FirstOrDefault(p => p.UserId == _currentUser.UserId.Value);
        if (existingPlayer == null)
        {
            await Clients.Caller.SendAsync("Error", "You must join the game via API first");
            return;
        }

        // Update connection ID
        existingPlayer.ConnectionId = Context.ConnectionId;
        await _context.SaveChangesAsync(default);

        // Join SignalR group
        await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);

        // IMPORTANT: Send ALL existing players to the newly joined caller
        // This ensures they get players who joined before them (whose PlayerJoined they missed)
        foreach (var player in game.Players)
        {
            var playerInfo = new PlayerInfo
            {
                UserId = player.UserId,
                Username = player.User.Username ?? player.User.DisplayName ?? "Player",
                PlayerIndex = player.PlayerIndex,
                IsReady = player.IsReady,
                SnakeColor = player.SnakeColor,
                SnakePositions = player.SnakePositions,
                Direction = player.Direction
            };

            await Clients.Caller.SendAsync("PlayerJoined", playerInfo);

            _logger.LogDebug("Sent existing player {PlayerIndex} ({Username}) to newly joined user {UserId} with {SnakeCount} snake positions",
                player.PlayerIndex, playerInfo.Username, _currentUser.UserId, player.SnakePositions?.Count ?? 0);
        }

        // Notify OTHERS in the group about the new player (they already have their own info)
        var newPlayerInfo = new PlayerInfo
        {
            UserId = existingPlayer.UserId,
            Username = existingPlayer.User.Username ?? existingPlayer.User.DisplayName ?? "Player",
            PlayerIndex = existingPlayer.PlayerIndex,
            IsReady = existingPlayer.IsReady,
            SnakeColor = existingPlayer.SnakeColor,
            SnakePositions = existingPlayer.SnakePositions,
            Direction = existingPlayer.Direction
        };

        await Clients.OthersInGroup(roomCode).SendAsync("PlayerJoined", newPlayerInfo);

        _logger.LogInformation("User {UserId} joined room {RoomCode} (sent {PlayerCount} existing players)",
            _currentUser.UserId, roomCode, game.Players.Count);
    }

    #region Matchmaking

    public async Task JoinMatchmaking(string mode, int playerCount)
    {
        if (!_currentUser.UserId.HasValue)
        {
            await Clients.Caller.SendAsync("Error", "User not authenticated");
            return;
        }

        if (!Enum.TryParse<MultiplayerGameMode>(mode, true, out var gameMode))
        {
            await Clients.Caller.SendAsync("MatchmakingError", new
            {
                Error = $"Invalid game mode: {mode}"
            });
            return;
        }

        var result = await _matchmakingService.JoinQueue(
            _currentUser.UserId.Value,
            Context.ConnectionId,
            gameMode,
            playerCount);

        if (result.Success)
        {
            await Clients.Caller.SendAsync("MatchmakingJoined", new
            {
                Mode = mode,
                PlayerCount = playerCount,
                QueuePosition = result.QueuePosition,
                EstimatedWaitSeconds = result.EstimatedWaitSeconds
            });

            _logger.LogInformation("User {UserId} joined matchmaking for {Mode} {PlayerCount}p",
                _currentUser.UserId, mode, playerCount);

            // Immediately try to create a match
            var match = await _matchmakingService.TryCreateMatchImmediately(gameMode, playerCount);

            if (match != null)
            {
                _logger.LogInformation("Immediate match created: {GameId} for {Mode} {PlayerCount}p",
                    match.GameId, mode, playerCount);

                // Notify all matched players
                foreach (var player in match.Players)
                {
                    if (!string.IsNullOrEmpty(player.ConnectionId))
                    {
                        await Clients.Client(player.ConnectionId).SendAsync("MatchFound", new
                        {
                            GameId = match.GameId,
                            RoomCode = match.RoomCode,
                            Mode = match.Mode,
                            PlayerCount = match.PlayerCount,
                            PlayerIndex = player.PlayerIndex
                        });

                        _logger.LogInformation("Notified player {UserId} of match {GameId}",
                            player.UserId, match.GameId);
                    }
                }
            }
        }
        else
        {
            await Clients.Caller.SendAsync("MatchmakingError", new
            {
                Error = result.Error
            });
        }
    }

    public async Task LeaveMatchmaking()
    {
        if (!_currentUser.UserId.HasValue)
            return;

        await _matchmakingService.LeaveQueue(_currentUser.UserId.Value);

        await Clients.Caller.SendAsync("MatchmakingLeft", new
        {
            Message = "Left matchmaking queue"
        });

        _logger.LogInformation("User {UserId} left matchmaking", _currentUser.UserId);
    }

    #endregion

    #region Reconnection

    public async Task Reconnect(string roomCode)
    {
        if (!_currentUser.UserId.HasValue)
        {
            await Clients.Caller.SendAsync("ReconnectFailed", new { Error = "User not authenticated" });
            return;
        }

        var game = await _context.MultiplayerGames
            .Include(g => g.Players)
            .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(g => g.RoomCode == roomCode);

        if (game == null)
        {
            await Clients.Caller.SendAsync("ReconnectFailed", new { Error = "Game not found" });
            return;
        }

        var player = game.Players.FirstOrDefault(p => p.UserId == _currentUser.UserId.Value);
        if (player == null)
        {
            await Clients.Caller.SendAsync("ReconnectFailed", new { Error = "You are not in this game" });
            return;
        }

        // Check if player can reconnect (within 60 second window)
        if (!player.CanReconnect && player.DisconnectedAt != null)
        {
            await Clients.Caller.SendAsync("ReconnectFailed", new { Error = "Reconnection window expired" });
            return;
        }

        // Check if game is still playable
        if (game.Status == MultiplayerGameStatus.Finished)
        {
            await Clients.Caller.SendAsync("ReconnectFailed", new { Error = "Game has ended" });
            return;
        }

        // Restore player connection
        player.ConnectionId = Context.ConnectionId;
        player.DisconnectedAt = null;
        await _context.SaveChangesAsync(default);

        // Rejoin SignalR group
        await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);

        // Send full game state to reconnected player
        var allPlayers = game.Players.Select(p => new
        {
            UserId = p.UserId,
            Username = p.User.Username ?? p.User.DisplayName ?? "Player",
            PlayerIndex = p.PlayerIndex,
            IsAlive = p.IsAlive,
            IsReady = p.IsReady,
            SnakeColor = p.SnakeColor,
            SnakePositions = p.SnakePositions,
            Direction = p.Direction,
            Score = p.Score,
            IsDisconnected = p.DisconnectedAt != null
        }).ToList();

        await Clients.Caller.SendAsync("ReconnectSuccess", new
        {
            GameId = game.GameId,
            RoomCode = game.RoomCode,
            Status = game.Status.ToString(),
            Mode = game.Mode.ToString(),
            YourPlayerIndex = player.PlayerIndex,
            Players = allPlayers,
            FoodPositions = game.FoodPositions,
            PowerUps = game.PowerUps,
            GameSettings = game.GameSettings
        });

        // Notify other players
        await Clients.OthersInGroup(roomCode).SendAsync("PlayerReconnected", new
        {
            UserId = _currentUser.UserId.Value,
            PlayerIndex = player.PlayerIndex
        });

        _logger.LogInformation("User {UserId} reconnected to game {RoomCode}", _currentUser.UserId, roomCode);
    }

    #endregion

    #region Heartbeat

    public async Task Ping()
    {
        await Clients.Caller.SendAsync("Pong", new
        {
            Timestamp = DateTime.UtcNow
        });
    }

    #endregion

    public async Task LeaveRoom(string roomCode)
    {
        if (!_currentUser.UserId.HasValue)
            return;

        var game = await _context.MultiplayerGames
            .Include(g => g.Players)
            .FirstOrDefaultAsync(g => g.RoomCode == roomCode);

        if (game == null)
            return;

        var player = game.Players.FirstOrDefault(p => p.UserId == _currentUser.UserId.Value);
        if (player == null)
            return;

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomCode);

        // Mark player as not alive (disconnected)
        player.IsAlive = false;
        await _context.SaveChangesAsync(default);

        await Clients.Group(roomCode).SendAsync("PlayerLeft", new
        {
            UserId = _currentUser.UserId.Value,
            PlayerIndex = player.PlayerIndex
        });

        // Check if all players left
        var activePlayers = game.Players.Count(p => p.IsAlive);
        if (activePlayers == 0 && game.Status != MultiplayerGameStatus.Finished)
        {
            game.Status = MultiplayerGameStatus.Finished;
            game.FinishedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(default);
        }

        _logger.LogInformation("User {UserId} left room {RoomCode}", _currentUser.UserId, roomCode);
    }

    public async Task SetReady(string roomCode, bool isReady)
    {
        if (!_currentUser.UserId.HasValue)
            return;

        var game = await _context.MultiplayerGames
            .Include(g => g.Players)
            .FirstOrDefaultAsync(g => g.RoomCode == roomCode);

        if (game == null || game.Status != MultiplayerGameStatus.Waiting)
            return;

        var player = game.Players.FirstOrDefault(p => p.UserId == _currentUser.UserId.Value);
        if (player == null)
            return;

        player.IsReady = isReady;
        await _context.SaveChangesAsync(default);

        await Clients.Group(roomCode).SendAsync("PlayerReady", new
        {
            UserId = _currentUser.UserId.Value,
            PlayerIndex = player.PlayerIndex,
            IsReady = isReady
        });

        _logger.LogInformation("User {UserId} set ready={IsReady} in room {RoomCode}",
            _currentUser.UserId, isReady, roomCode);
    }

    public async Task StartGame(string roomCode)
    {
        _logger.LogInformation("StartGame called by user {UserId} for room {RoomCode}",
            _currentUser.UserId, roomCode);

        if (!_currentUser.UserId.HasValue)
        {
            _logger.LogWarning("StartGame failed: User not authenticated");
            await Clients.Caller.SendAsync("Error", "User not authenticated");
            return;
        }

        var game = await _context.MultiplayerGames
            .Include(g => g.Players)
            .FirstOrDefaultAsync(g => g.RoomCode == roomCode);

        if (game == null)
        {
            _logger.LogWarning("StartGame failed: Game not found for room {RoomCode}", roomCode);
            await Clients.Caller.SendAsync("Error", "Game not found");
            return;
        }

        _logger.LogDebug("StartGame: Game found - HostId={HostId}, Status={Status}, PlayerCount={PlayerCount}",
            game.HostId, game.Status, game.Players.Count);

        // Only host can start the game
        if (game.HostId != _currentUser.UserId.Value)
        {
            _logger.LogWarning("StartGame failed: User {UserId} is not host (host is {HostId})",
                _currentUser.UserId, game.HostId);
            await Clients.Caller.SendAsync("Error", "Only the host can start the game");
            return;
        }

        if (game.Status != MultiplayerGameStatus.Waiting)
        {
            _logger.LogWarning("StartGame failed: Game status is {Status}, not Waiting", game.Status);
            await Clients.Caller.SendAsync("Error", "Game cannot be started");
            return;
        }

        // Check if all players are ready
        var allReady = game.Players.All(p => p.IsReady);
        if (!allReady)
        {
            var notReadyPlayers = game.Players.Where(p => !p.IsReady).Select(p => p.UserId);
            _logger.LogWarning("StartGame failed: Not all players ready. Not ready: {NotReady}",
                string.Join(", ", notReadyPlayers));
            await Clients.Caller.SendAsync("Error", "Not all players are ready");
            return;
        }

        // Start countdown
        game.Status = MultiplayerGameStatus.Countdown;
        await _context.SaveChangesAsync(default);

        _logger.LogInformation("Game {RoomCode} starting countdown - broadcasting GameStarting to {PlayerCount} players",
            roomCode, game.Players.Count);

        await Clients.Group(roomCode).SendAsync("GameStarting", new
        {
            CountdownSeconds = 3
        });

        // Capture values needed for background task
        var gameId = game.Id;
        var capturedRoomCode = roomCode;
        var scopeFactory = _serviceScopeFactory;
        var hubContext = _hubContext;

        // After countdown, set to playing using proper scoped services
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(3000);

                // Create a new scope for the background operation
                using var scope = scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<GameHub>>();

                // Re-fetch the game in the new scope with players
                var gameToUpdate = await dbContext.MultiplayerGames
                    .Include(g => g.Players)
                    .ThenInclude(p => p.User)
                    .FirstOrDefaultAsync(g => g.Id == gameId);

                if (gameToUpdate != null)
                {
                    gameToUpdate.Status = MultiplayerGameStatus.Playing;
                    gameToUpdate.StartedAt = DateTime.UtcNow;
                    await dbContext.SaveChangesAsync(default);

                    logger.LogInformation("Game {RoomCode} started - broadcasting GameStarted with {PlayerCount} players",
                        capturedRoomCode, gameToUpdate.Players.Count);

                    // Build player data with snake positions
                    var playerData = gameToUpdate.Players.Select(p => new
                    {
                        UserId = p.UserId,
                        Username = p.User.Username ?? p.User.DisplayName ?? "Player",
                        PlayerIndex = p.PlayerIndex,
                        SnakePositions = p.SnakePositions,
                        Direction = p.Direction,
                        SnakeColor = p.SnakeColor,
                        Score = p.Score,
                        IsAlive = p.IsAlive
                    }).ToList();

                    // Use IHubContext to broadcast (works outside of hub method)
                    await hubContext.Clients.Group(capturedRoomCode).SendAsync("GameStarted", new
                    {
                        StartedAt = gameToUpdate.StartedAt,
                        FoodPositions = gameToUpdate.FoodPositions,
                        PowerUps = gameToUpdate.PowerUps,
                        Players = playerData,
                        BoardSize = gameToUpdate.GameSettings.ContainsKey("boardSize")
                            ? gameToUpdate.GameSettings["boardSize"]
                            : 20,
                        GameSettings = gameToUpdate.GameSettings
                    });
                }
                else
                {
                    logger.LogWarning("Game {GameId} not found when trying to start", gameId);
                }
            }
            catch (Exception ex)
            {
                // Log any errors in the background task
                using var scope = scopeFactory.CreateScope();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<GameHub>>();
                logger.LogError(ex, "Error in background task for starting game {RoomCode}", capturedRoomCode);
            }
        });

        _logger.LogInformation("Game {RoomCode} countdown initiated by user {UserId}", roomCode, _currentUser.UserId);
    }

    public async Task SendMove(string roomCode, MoveData moveData)
    {
        if (!_currentUser.UserId.HasValue)
            return;

        var game = await _context.MultiplayerGames
            .Include(g => g.Players)
            .FirstOrDefaultAsync(g => g.RoomCode == roomCode);

        if (game == null || game.Status != MultiplayerGameStatus.Playing)
            return;

        var player = game.Players.FirstOrDefault(p => p.UserId == _currentUser.UserId.Value);
        if (player == null || !player.IsAlive)
            return;

        // Update player state
        player.Direction = moveData.Direction;
        player.SnakePositions = moveData.SnakePositions;
        player.Score = moveData.Score;
        player.LastUpdateAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(default);

        // Broadcast to other players
        await Clients.OthersInGroup(roomCode).SendAsync("PlayerMoved", new
        {
            UserId = _currentUser.UserId.Value,
            PlayerIndex = player.PlayerIndex,
            Direction = moveData.Direction,
            SnakePositions = moveData.SnakePositions,
            Score = moveData.Score,
            Timestamp = DateTime.UtcNow
        });
    }

    public async Task UpdateGameState(string roomCode, GameStateUpdate update)
    {
        if (!_currentUser.UserId.HasValue)
            return;

        var game = await _context.MultiplayerGames
            .FirstOrDefaultAsync(g => g.RoomCode == roomCode);

        if (game == null || game.Status != MultiplayerGameStatus.Playing)
            return;

        // Only host can update game state (food positions, power-ups)
        if (game.HostId != _currentUser.UserId.Value)
            return;

        if (update.FoodPositions != null)
            game.FoodPositions = update.FoodPositions;

        if (update.PowerUps != null)
            game.PowerUps = update.PowerUps;

        await _context.SaveChangesAsync(default);

        await Clients.Group(roomCode).SendAsync("GameStateUpdated", new
        {
            FoodPositions = game.FoodPositions,
            PowerUps = game.PowerUps,
            Timestamp = DateTime.UtcNow
        });
    }

    public async Task PlayerDied(string roomCode)
    {
        if (!_currentUser.UserId.HasValue)
            return;

        var game = await _context.MultiplayerGames
            .Include(g => g.Players)
            .FirstOrDefaultAsync(g => g.RoomCode == roomCode);

        if (game == null || game.Status != MultiplayerGameStatus.Playing)
            return;

        var player = game.Players.FirstOrDefault(p => p.UserId == _currentUser.UserId.Value);
        if (player == null || !player.IsAlive)
            return;

        // Calculate elimination rank (higher is worse - last to die is better)
        var aliveBefore = game.Players.Count(p => p.IsAlive);
        var eliminationRank = aliveBefore; // If 4 alive, dying now means rank 4 (4th place)

        player.IsAlive = false;
        player.EliminatedAt = DateTime.UtcNow;
        player.EliminationRank = eliminationRank;
        await _context.SaveChangesAsync(default);

        await Clients.Group(roomCode).SendAsync("PlayerEliminated", new
        {
            UserId = _currentUser.UserId.Value,
            PlayerIndex = player.PlayerIndex,
            FinalScore = player.Score,
            EliminationRank = eliminationRank,
            PlayersRemaining = aliveBefore - 1
        });

        // Check if game should end (only one or zero players alive)
        var alivePlayers = game.Players.Where(p => p.IsAlive).ToList();
        if (alivePlayers.Count <= 1)
        {
            // Winner is the last player standing
            var winner = alivePlayers.FirstOrDefault();
            if (winner != null)
            {
                winner.EliminationRank = 1; // 1st place
                winner.EliminatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(default);
            }
            await EndGame(game, winner?.UserId);
        }

        _logger.LogInformation("Player {UserId} eliminated (rank {Rank}) in room {RoomCode}",
            _currentUser.UserId, eliminationRank, roomCode);
    }

    public async Task GameOver(string roomCode, int finalScore)
    {
        if (!_currentUser.UserId.HasValue)
            return;

        var game = await _context.MultiplayerGames
            .Include(g => g.Players)
            .FirstOrDefaultAsync(g => g.RoomCode == roomCode);

        if (game == null)
            return;

        var player = game.Players.FirstOrDefault(p => p.UserId == _currentUser.UserId.Value);
        if (player == null)
            return;

        player.Score = finalScore;
        player.IsAlive = false;
        await _context.SaveChangesAsync(default);

        // Check if all players finished
        var alivePlayers = game.Players.Count(p => p.IsAlive);
        if (alivePlayers == 0)
        {
            await EndGame(game, null);
        }
    }

    private async Task EndGame(Domain.Entities.MultiplayerGame game, Guid? winnerId)
    {
        game.Status = MultiplayerGameStatus.Finished;
        game.FinishedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(default);

        // For multi-player games, use elimination rank; for 2-player, use score
        var results = game.Players
            .OrderBy(p => p.EliminationRank ?? int.MaxValue) // Winner (rank 1) first
            .ThenByDescending(p => p.Score) // Tie-break by score
            .Select((p, index) => new PlayerResult
            {
                UserId = p.UserId,
                PlayerIndex = p.PlayerIndex,
                Score = p.Score,
                Rank = p.EliminationRank ?? (index + 1)
            })
            .ToList();

        await Clients.Group(game.RoomCode).SendAsync("GameEnded", new
        {
            WinnerId = winnerId ?? results.FirstOrDefault()?.UserId,
            Results = results,
            FinishedAt = game.FinishedAt,
            TotalPlayers = game.Players.Count
        });

        _logger.LogInformation("Game {RoomCode} ended with {PlayerCount} players", game.RoomCode, game.Players.Count);
    }
}

// DTOs for SignalR messages
public class PlayerInfo
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = null!;
    public int PlayerIndex { get; set; }
    public bool IsReady { get; set; }
    public string? SnakeColor { get; set; }
    public List<Dictionary<string, object>>? SnakePositions { get; set; }
    public string? Direction { get; set; }
}

public class MoveData
{
    public string Direction { get; set; } = null!;
    public List<Dictionary<string, object>>? SnakePositions { get; set; }
    public int Score { get; set; }
}

public class GameStateUpdate
{
    public List<Dictionary<string, object>>? FoodPositions { get; set; }
    public List<Dictionary<string, object>>? PowerUps { get; set; }
}

public class PlayerResult
{
    public Guid UserId { get; set; }
    public int PlayerIndex { get; set; }
    public int Score { get; set; }
    public int Rank { get; set; }
}
