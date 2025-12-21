using SnakeClassic.Domain.Common;
using SnakeClassic.Domain.Enums;

namespace SnakeClassic.Domain.Entities;

public class User : BaseEntity
{
    public string? FirebaseUid { get; set; }
    public string? Email { get; set; }
    public string? Username { get; set; }
    public string? DisplayName { get; set; }
    public string? PhotoUrl { get; set; }
    public AuthProvider AuthProvider { get; set; } = AuthProvider.Google;
    public bool IsAnonymous { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Offline;
    public string? StatusMessage { get; set; }
    public bool IsPublic { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public int HighScore { get; set; }
    public int TotalGamesPlayed { get; set; }
    public long TotalScore { get; set; }
    public int Level { get; set; } = 1;
    public int Coins { get; set; }
    public DateTime JoinedDate { get; set; } = DateTime.UtcNow;
    public DateTime LastSeen { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public UserPreferences? Preferences { get; set; }
    public UserPremiumContent? PremiumContent { get; set; }
    public ICollection<Score> Scores { get; set; } = new List<Score>();
    public ICollection<UserAchievement> Achievements { get; set; } = new List<UserAchievement>();
    public ICollection<FcmToken> FcmTokens { get; set; } = new List<FcmToken>();
    public ICollection<Friendship> SentFriendRequests { get; set; } = new List<Friendship>();
    public ICollection<Friendship> ReceivedFriendRequests { get; set; } = new List<Friendship>();
    public ICollection<TournamentEntry> TournamentEntries { get; set; } = new List<TournamentEntry>();
    public ICollection<MultiplayerPlayer> MultiplayerGames { get; set; } = new List<MultiplayerPlayer>();
    public ICollection<UserBattlePassProgress> BattlePassProgress { get; set; } = new List<UserBattlePassProgress>();
    public ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
    public ICollection<GameReplay> GameReplays { get; set; } = new List<GameReplay>();

    public bool IsPremium => PremiumContent?.SubscriptionActive == true ||
                             PremiumContent?.PremiumTier != "free";
}
