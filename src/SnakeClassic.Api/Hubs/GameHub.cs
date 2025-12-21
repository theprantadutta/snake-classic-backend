using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SnakeClassic.Application.Common.Interfaces;
using SnakeClassic.Domain.Enums;

namespace SnakeClassic.Api.Hubs;

[Authorize]
public class GameHub : Hub
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<GameHub> _logger;

    public GameHub(
        IAppDbContext context,
        ICurrentUserService currentUser,
        ILogger<GameHub> logger)
    {
        _context = context;
        _currentUser = currentUser;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("User {UserId} connected to GameHub", _currentUser.UserId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("User {UserId} disconnected from GameHub", _currentUser.UserId);

        // Handle player disconnection - leave any active game
        if (_currentUser.UserId.HasValue)
        {
            var player = await _context.MultiplayerPlayers
                .Include(p => p.Game)
                .FirstOrDefaultAsync(p => p.UserId == _currentUser.UserId.Value &&
                    p.Game.Status != MultiplayerGameStatus.Finished);

            if (player != null)
            {
                await LeaveRoom(player.Game.RoomCode);
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

        await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);

        var playerInfo = new PlayerInfo
        {
            UserId = existingPlayer.UserId,
            Username = existingPlayer.User.Username ?? existingPlayer.User.DisplayName ?? "Player",
            PlayerIndex = existingPlayer.PlayerIndex,
            IsReady = existingPlayer.IsReady,
            SnakeColor = existingPlayer.SnakeColor
        };

        await Clients.Group(roomCode).SendAsync("PlayerJoined", playerInfo);

        _logger.LogInformation("User {UserId} joined room {RoomCode}", _currentUser.UserId, roomCode);
    }

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
        if (!_currentUser.UserId.HasValue)
            return;

        var game = await _context.MultiplayerGames
            .Include(g => g.Players)
            .FirstOrDefaultAsync(g => g.RoomCode == roomCode);

        if (game == null)
        {
            await Clients.Caller.SendAsync("Error", "Game not found");
            return;
        }

        // Only host can start the game
        if (game.HostId != _currentUser.UserId.Value)
        {
            await Clients.Caller.SendAsync("Error", "Only the host can start the game");
            return;
        }

        if (game.Status != MultiplayerGameStatus.Waiting)
        {
            await Clients.Caller.SendAsync("Error", "Game cannot be started");
            return;
        }

        // Check if all players are ready
        var allReady = game.Players.All(p => p.IsReady);
        if (!allReady)
        {
            await Clients.Caller.SendAsync("Error", "Not all players are ready");
            return;
        }

        // Start countdown
        game.Status = MultiplayerGameStatus.Countdown;
        await _context.SaveChangesAsync(default);

        await Clients.Group(roomCode).SendAsync("GameStarting", new
        {
            CountdownSeconds = 3
        });

        // After countdown, set to playing
        _ = Task.Run(async () =>
        {
            await Task.Delay(3000);

            game.Status = MultiplayerGameStatus.Playing;
            game.StartedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(default);

            await Clients.Group(roomCode).SendAsync("GameStarted", new
            {
                StartedAt = game.StartedAt,
                FoodPositions = game.FoodPositions,
                PowerUps = game.PowerUps
            });
        });

        _logger.LogInformation("Game {RoomCode} started by user {UserId}", roomCode, _currentUser.UserId);
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
        if (player == null)
            return;

        player.IsAlive = false;
        await _context.SaveChangesAsync(default);

        await Clients.Group(roomCode).SendAsync("PlayerDied", new
        {
            UserId = _currentUser.UserId.Value,
            PlayerIndex = player.PlayerIndex,
            FinalScore = player.Score
        });

        // Check if game should end (only one or zero players alive)
        var alivePlayers = game.Players.Where(p => p.IsAlive).ToList();
        if (alivePlayers.Count <= 1)
        {
            await EndGame(game, alivePlayers.FirstOrDefault()?.UserId);
        }

        _logger.LogInformation("Player {UserId} died in room {RoomCode}", _currentUser.UserId, roomCode);
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

        var results = game.Players
            .OrderByDescending(p => p.Score)
            .Select((p, index) => new PlayerResult
            {
                UserId = p.UserId,
                PlayerIndex = p.PlayerIndex,
                Score = p.Score,
                Rank = index + 1
            })
            .ToList();

        await Clients.Group(game.RoomCode).SendAsync("GameEnded", new
        {
            WinnerId = winnerId ?? results.FirstOrDefault()?.UserId,
            Results = results,
            FinishedAt = game.FinishedAt
        });

        _logger.LogInformation("Game {RoomCode} ended", game.RoomCode);
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
