using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SnakeClassic.Application.Common.Interfaces;
using SnakeClassic.Domain.Entities;
using SnakeClassic.Domain.Enums;

namespace SnakeClassic.Infrastructure.Services.BackgroundJobs;

public interface ITournamentManagementJobService
{
    Task CreateDailyTournament();
    Task CreateWeeklyTournament();
    Task CreateMonthlyTournament();
    Task ProcessTournamentLifecycle();
    Task DistributeTournamentPrizes();
}

public class TournamentManagementJobService : ITournamentManagementJobService
{
    private readonly IAppDbContext _dbContext;
    private readonly INotificationJobService _notificationService;
    private readonly ILogger<TournamentManagementJobService> _logger;

    public TournamentManagementJobService(
        IAppDbContext dbContext,
        INotificationJobService notificationService,
        ILogger<TournamentManagementJobService> logger)
    {
        _dbContext = dbContext;
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new daily tournament. Runs at midnight UTC.
    /// Daily tournaments run for 24 hours.
    /// </summary>
    public async Task CreateDailyTournament()
    {
        try
        {
            var today = DateTime.UtcNow.Date;
            var tournamentId = $"daily-{today:yyyy-MM-dd}";

            // Check if today's tournament already exists
            var existingTournament = await _dbContext.Tournaments
                .FirstOrDefaultAsync(t => t.TournamentId == tournamentId);

            if (existingTournament != null)
            {
                _logger.LogInformation("Daily tournament {TournamentId} already exists, skipping creation", tournamentId);
                return;
            }

            var tournament = new Tournament
            {
                TournamentId = tournamentId,
                Name = $"Daily Challenge - {today:MMMM dd}",
                Description = "Compete for the highest score in today's 24-hour challenge! Top players win coins and glory.",
                Type = TournamentType.Daily,
                Status = TournamentStatus.Active, // Daily tournaments start immediately
                StartDate = today,
                EndDate = today.AddDays(1),
                EntryFee = 0, // Free entry for daily tournaments
                MinLevel = 1,
                MaxPlayers = 1000,
                MaxParticipants = 1000,
                PrizePool = 5000,
                PrizeDistribution = new Dictionary<string, object>
                {
                    { "1", 2000 },
                    { "2", 1000 },
                    { "3", 500 },
                    { "4", 300 },
                    { "5", 200 },
                    { "6-10", 100 },
                    { "11-20", 50 },
                    { "21-50", 25 }
                },
                Rules = new Dictionary<string, object>
                {
                    { "gameMode", "Classic" },
                    { "maxAttempts", 0 }, // Unlimited attempts
                    { "bestScoreWins", true },
                    { "description", "Play as many times as you want. Your best score counts!" }
                },
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Tournaments.Add(tournament);
            await _dbContext.SaveChangesAsync(default);

            _logger.LogInformation("Created daily tournament: {TournamentId} - {Name}", tournamentId, tournament.Name);

            // Send notification about new tournament
            await _notificationService.SendTournamentStarted(tournamentId, tournament.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create daily tournament");
            throw;
        }
    }

    /// <summary>
    /// Creates a new weekly tournament. Runs every Monday at midnight UTC.
    /// Weekly tournaments run for 7 days.
    /// </summary>
    public async Task CreateWeeklyTournament()
    {
        try
        {
            var today = DateTime.UtcNow.Date;
            // Get the Monday of this week
            var monday = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
            if (today.DayOfWeek == DayOfWeek.Sunday)
                monday = monday.AddDays(-7); // Handle Sunday edge case

            var weekNumber = GetIso8601WeekOfYear(monday);
            var tournamentId = $"weekly-{monday.Year}-W{weekNumber:D2}";

            // Check if this week's tournament already exists
            var existingTournament = await _dbContext.Tournaments
                .FirstOrDefaultAsync(t => t.TournamentId == tournamentId);

            if (existingTournament != null)
            {
                _logger.LogInformation("Weekly tournament {TournamentId} already exists, skipping creation", tournamentId);
                return;
            }

            var tournament = new Tournament
            {
                TournamentId = tournamentId,
                Name = $"Weekly Championship - Week {weekNumber}",
                Description = "The ultimate weekly showdown! Compete against the best players for massive rewards.",
                Type = TournamentType.Weekly,
                Status = TournamentStatus.Active,
                StartDate = monday,
                EndDate = monday.AddDays(7),
                EntryFee = 100, // 100 coins entry fee
                MinLevel = 3, // Requires level 3
                MaxPlayers = 500,
                MaxParticipants = 500,
                PrizePool = 25000,
                PrizeDistribution = new Dictionary<string, object>
                {
                    { "1", 10000 },
                    { "2", 5000 },
                    { "3", 2500 },
                    { "4", 1500 },
                    { "5", 1000 },
                    { "6-10", 500 },
                    { "11-25", 200 },
                    { "26-50", 100 },
                    { "51-100", 50 }
                },
                Rules = new Dictionary<string, object>
                {
                    { "gameMode", "Classic" },
                    { "maxAttempts", 0 }, // Unlimited
                    { "bestScoreWins", true },
                    { "description", "Entry fee: 100 coins. Top 100 players win prizes!" }
                },
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Tournaments.Add(tournament);
            await _dbContext.SaveChangesAsync(default);

            _logger.LogInformation("Created weekly tournament: {TournamentId} - {Name}", tournamentId, tournament.Name);

            await _notificationService.SendTournamentStarted(tournamentId, tournament.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create weekly tournament");
            throw;
        }
    }

    /// <summary>
    /// Creates a new monthly tournament. Runs on the 1st of each month.
    /// Monthly tournaments run for the entire month.
    /// </summary>
    public async Task CreateMonthlyTournament()
    {
        try
        {
            var today = DateTime.UtcNow.Date;
            var firstOfMonth = new DateTime(today.Year, today.Month, 1);
            var lastOfMonth = firstOfMonth.AddMonths(1).AddDays(-1);
            var tournamentId = $"monthly-{firstOfMonth:yyyy-MM}";

            // Check if this month's tournament already exists
            var existingTournament = await _dbContext.Tournaments
                .FirstOrDefaultAsync(t => t.TournamentId == tournamentId);

            if (existingTournament != null)
            {
                _logger.LogInformation("Monthly tournament {TournamentId} already exists, skipping creation", tournamentId);
                return;
            }

            var tournament = new Tournament
            {
                TournamentId = tournamentId,
                Name = $"Monthly Grand Prix - {firstOfMonth:MMMM yyyy}",
                Description = "The biggest tournament of the month! Prove you're the ultimate Snake master.",
                Type = TournamentType.Special, // Using Special for monthly
                Status = TournamentStatus.Active,
                StartDate = firstOfMonth,
                EndDate = lastOfMonth.AddDays(1), // End at midnight after last day
                EntryFee = 250, // 250 coins entry fee
                MinLevel = 5, // Requires level 5
                MaxPlayers = 1000,
                MaxParticipants = 1000,
                PrizePool = 100000,
                PrizeDistribution = new Dictionary<string, object>
                {
                    { "1", 40000 },
                    { "2", 20000 },
                    { "3", 10000 },
                    { "4", 5000 },
                    { "5", 3000 },
                    { "6-10", 2000 },
                    { "11-25", 500 },
                    { "26-50", 250 },
                    { "51-100", 100 }
                },
                Rules = new Dictionary<string, object>
                {
                    { "gameMode", "Classic" },
                    { "maxAttempts", 0 },
                    { "bestScoreWins", true },
                    { "description", "Entry fee: 250 coins. Massive prizes for top 100 players!" }
                },
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Tournaments.Add(tournament);
            await _dbContext.SaveChangesAsync(default);

            _logger.LogInformation("Created monthly tournament: {TournamentId} - {Name}", tournamentId, tournament.Name);

            await _notificationService.SendTournamentStarted(tournamentId, tournament.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create monthly tournament");
            throw;
        }
    }

    /// <summary>
    /// Processes tournament lifecycle transitions.
    /// - Upcoming -> Active when StartDate is reached
    /// - Active -> Completed when EndDate is passed
    /// Runs every 5 minutes.
    /// </summary>
    public async Task ProcessTournamentLifecycle()
    {
        try
        {
            var now = DateTime.UtcNow;
            var transitioned = 0;

            // Transition Upcoming -> Active
            var tournamentsToActivate = await _dbContext.Tournaments
                .Where(t => t.Status == TournamentStatus.Upcoming && t.StartDate <= now)
                .ToListAsync();

            foreach (var tournament in tournamentsToActivate)
            {
                tournament.Status = TournamentStatus.Active;
                _logger.LogInformation("Tournament {TournamentId} transitioned to Active", tournament.TournamentId);

                // Notify users
                await _notificationService.SendTournamentStarted(tournament.TournamentId, tournament.Name);
                transitioned++;
            }

            // Transition Active -> Completed
            var tournamentsToComplete = await _dbContext.Tournaments
                .Where(t => t.Status == TournamentStatus.Active && t.EndDate <= now)
                .ToListAsync();

            foreach (var tournament in tournamentsToComplete)
            {
                tournament.Status = TournamentStatus.Completed;
                _logger.LogInformation("Tournament {TournamentId} transitioned to Completed", tournament.TournamentId);
                transitioned++;
            }

            if (transitioned > 0)
            {
                await _dbContext.SaveChangesAsync(default);
                _logger.LogInformation("Processed {Count} tournament lifecycle transitions", transitioned);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process tournament lifecycle");
            throw;
        }
    }

    /// <summary>
    /// Distributes prizes for completed tournaments that haven't been processed yet.
    /// Runs every 15 minutes.
    /// </summary>
    public async Task DistributeTournamentPrizes()
    {
        try
        {
            // Find completed tournaments with undistributed prizes
            var completedTournaments = await _dbContext.Tournaments
                .Include(t => t.Entries)
                    .ThenInclude(e => e.User)
                .Where(t => t.Status == TournamentStatus.Completed)
                .Where(t => t.Entries.Any(e => e.Rank == null)) // Has unranked entries
                .ToListAsync();

            foreach (var tournament in completedTournaments)
            {
                await ProcessTournamentPrizes(tournament);
            }

            if (completedTournaments.Count > 0)
            {
                _logger.LogInformation("Processed prizes for {Count} tournaments", completedTournaments.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to distribute tournament prizes");
            throw;
        }
    }

    private async Task ProcessTournamentPrizes(Tournament tournament)
    {
        try
        {
            _logger.LogInformation("Processing prizes for tournament {TournamentId}", tournament.TournamentId);

            // Rank all entries by best score (descending)
            var rankedEntries = tournament.Entries
                .OrderByDescending(e => e.BestScore)
                .ThenBy(e => e.JoinedAt) // Tie-breaker: earlier join wins
                .ToList();

            var prizeDistribution = tournament.PrizeDistribution ?? new Dictionary<string, object>();

            for (int i = 0; i < rankedEntries.Count; i++)
            {
                var entry = rankedEntries[i];
                var rank = i + 1;
                entry.Rank = rank;

                // Calculate prize
                var prize = CalculatePrize(rank, prizeDistribution);
                if (prize > 0)
                {
                    entry.PrizeAmount = new Dictionary<string, object> { { "coins", prize } };

                    // Award coins to user
                    if (entry.User != null)
                    {
                        entry.User.Coins += prize;
                        _logger.LogInformation(
                            "Awarded {Prize} coins to user {UserId} for rank {Rank} in tournament {TournamentId}",
                            prize, entry.UserId, rank, tournament.TournamentId);
                    }
                }
            }

            await _dbContext.SaveChangesAsync(default);

            _logger.LogInformation(
                "Completed prize distribution for tournament {TournamentId}. {EntryCount} entries processed.",
                tournament.TournamentId, rankedEntries.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process prizes for tournament {TournamentId}", tournament.TournamentId);
        }
    }

    private static int CalculatePrize(int rank, Dictionary<string, object> prizeDistribution)
    {
        // Check exact rank match first
        if (prizeDistribution.TryGetValue(rank.ToString(), out var exactPrize))
        {
            return Convert.ToInt32(exactPrize);
        }

        // Check range matches (e.g., "6-10", "11-25")
        foreach (var kvp in prizeDistribution)
        {
            if (kvp.Key.Contains('-'))
            {
                var parts = kvp.Key.Split('-');
                if (parts.Length == 2 &&
                    int.TryParse(parts[0], out var min) &&
                    int.TryParse(parts[1], out var max) &&
                    rank >= min && rank <= max)
                {
                    return Convert.ToInt32(kvp.Value);
                }
            }
        }

        return 0; // No prize for this rank
    }

    private static int GetIso8601WeekOfYear(DateTime date)
    {
        var day = System.Globalization.CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(date);
        if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
        {
            date = date.AddDays(3);
        }
        return System.Globalization.CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(
            date, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
    }
}
