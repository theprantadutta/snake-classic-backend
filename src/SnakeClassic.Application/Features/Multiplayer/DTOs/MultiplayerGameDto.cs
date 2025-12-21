using SnakeClassic.Domain.Enums;

namespace SnakeClassic.Application.Features.Multiplayer.DTOs;

public record MultiplayerGameDto(
    Guid Id,
    string GameId,
    MultiplayerGameMode Mode,
    MultiplayerGameStatus Status,
    string RoomCode,
    int MaxPlayers,
    int CurrentPlayers,
    Guid? HostId,
    List<MultiplayerPlayerDto> Players
);

public record MultiplayerPlayerDto(
    Guid UserId,
    string? Username,
    string? DisplayName,
    string? PhotoUrl,
    int PlayerIndex,
    int Score,
    bool IsReady,
    bool IsAlive
);

public record CreateGameResultDto(
    Guid GameId,
    string RoomCode
);

public record JoinGameResultDto(
    Guid GameId,
    int PlayerIndex,
    List<MultiplayerPlayerDto> Players
);
