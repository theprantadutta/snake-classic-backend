using SnakeClassic.Domain.Enums;

namespace SnakeClassic.Application.Features.Tournaments.DTOs;

public record TournamentDto(
    Guid Id,
    string TournamentId,
    string Name,
    string Description,
    TournamentStatus Status,
    DateTime StartDate,
    DateTime EndDate,
    int EntryFee,
    int PrizePool,
    int MaxParticipants,
    int CurrentParticipants,
    bool IsJoined
);

public record TournamentEntryDto(
    int Rank,
    Guid UserId,
    string? Username,
    string? DisplayName,
    string? PhotoUrl,
    int BestScore,
    int GamesPlayed,
    bool PrizeClaimed
);

public record TournamentLeaderboardDto(
    TournamentDto Tournament,
    List<TournamentEntryDto> Entries,
    int? CurrentUserRank
);

public record TournamentsResponseDto(
    List<TournamentDto> Tournaments
);
