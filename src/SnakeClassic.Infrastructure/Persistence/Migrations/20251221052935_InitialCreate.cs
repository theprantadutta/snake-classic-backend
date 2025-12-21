using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SnakeClassic.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "achievements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    achievement_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    icon = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    tier = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    requirement_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    requirement_value = table.Column<int>(type: "integer", nullable: false),
                    xp_reward = table.Column<int>(type: "integer", nullable: false),
                    coin_reward = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_secret = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_achievements", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "battle_pass_seasons",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    season_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    theme = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    theme_color = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    max_level = table.Column<int>(type: "integer", nullable: false),
                    price = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    levels_config = table.Column<List<Dictionary<string, object>>>(type: "jsonb", nullable: true),
                    extra_data = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_battle_pass_seasons", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "multiplayer_games",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    game_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    mode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    room_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    max_players = table.Column<int>(type: "integer", nullable: false),
                    host_id = table.Column<Guid>(type: "uuid", nullable: true),
                    food_positions = table.Column<List<Dictionary<string, object>>>(type: "jsonb", nullable: true),
                    power_ups = table.Column<List<Dictionary<string, object>>>(type: "jsonb", nullable: true),
                    game_settings = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    finished_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_multiplayer_games", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "notification_histories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    notification_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    body = table.Column<string>(type: "text", nullable: false),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    priority = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    recipient_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    recipients_count = table.Column<int>(type: "integer", nullable: false),
                    success_count = table.Column<int>(type: "integer", nullable: false),
                    failure_count = table.Column<int>(type: "integer", nullable: false),
                    extra_data = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: true),
                    sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_histories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "scheduled_jobs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    job_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    job_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    trigger_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    trigger_config = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: true),
                    job_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    payload = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: true),
                    next_run_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scheduled_jobs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tournaments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tournament_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    entry_fee = table.Column<int>(type: "integer", nullable: false),
                    min_level = table.Column<int>(type: "integer", nullable: false),
                    max_players = table.Column<int>(type: "integer", nullable: false),
                    max_participants = table.Column<int>(type: "integer", nullable: false),
                    prize_pool = table.Column<int>(type: "integer", nullable: false),
                    prize_distribution = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: true),
                    rules = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tournaments", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    firebase_uid = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    username = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    display_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    photo_url = table.Column<string>(type: "text", nullable: true),
                    auth_provider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_anonymous = table.Column<bool>(type: "boolean", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status_message = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    is_public = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    high_score = table.Column<int>(type: "integer", nullable: false),
                    total_games_played = table.Column<int>(type: "integer", nullable: false),
                    total_score = table.Column<long>(type: "bigint", nullable: false),
                    level = table.Column<int>(type: "integer", nullable: false),
                    coins = table.Column<int>(type: "integer", nullable: false),
                    joined_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_seen = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_active_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "fcm_tokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    platform = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    subscribed_topics = table.Column<string>(type: "jsonb", nullable: false),
                    registered_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_used_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fcm_tokens", x => x.id);
                    table.ForeignKey(
                        name: "f_k_fcm_tokens__users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "friendships",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    friend_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_friendships", x => x.id);
                    table.ForeignKey(
                        name: "f_k_friendships__users_friend_id",
                        column: x => x.friend_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "f_k_friendships__users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "multiplayer_players",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    game_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    player_index = table.Column<int>(type: "integer", nullable: false),
                    score = table.Column<int>(type: "integer", nullable: false),
                    is_alive = table.Column<bool>(type: "boolean", nullable: false),
                    is_ready = table.Column<bool>(type: "boolean", nullable: false),
                    snake_positions = table.Column<List<Dictionary<string, object>>>(type: "jsonb", nullable: true),
                    direction = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    snake_color = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    joined_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_update_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_multiplayer_players", x => x.id);
                    table.ForeignKey(
                        name: "f_k_multiplayer_players__users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "f_k_multiplayer_players_multiplayer_games_game_id",
                        column: x => x.game_id,
                        principalTable: "multiplayer_games",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "purchases",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    transaction_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    platform = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    receipt_data = table.Column<string>(type: "text", nullable: true),
                    is_verified = table.Column<bool>(type: "boolean", nullable: false),
                    verification_error = table.Column<string>(type: "text", nullable: true),
                    is_subscription = table.Column<bool>(type: "boolean", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    auto_renewing = table.Column<bool>(type: "boolean", nullable: false),
                    content_unlocked = table.Column<string>(type: "jsonb", nullable: true),
                    purchase_timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    verified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_purchases", x => x.id);
                    table.ForeignKey(
                        name: "f_k_purchases__users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "scores",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    score_value = table.Column<int>(type: "integer", nullable: false),
                    game_duration_seconds = table.Column<int>(type: "integer", nullable: false),
                    foods_eaten = table.Column<int>(type: "integer", nullable: false),
                    game_mode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    difficulty = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    game_data = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: true),
                    idempotency_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    played_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scores", x => x.id);
                    table.ForeignKey(
                        name: "f_k_scores__users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tournament_entries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tournament_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    best_score = table.Column<int>(type: "integer", nullable: false),
                    games_played = table.Column<int>(type: "integer", nullable: false),
                    rank = table.Column<int>(type: "integer", nullable: true),
                    prize_claimed = table.Column<bool>(type: "boolean", nullable: false),
                    prize_amount = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: true),
                    joined_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_played_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tournament_entries", x => x.id);
                    table.ForeignKey(
                        name: "f_k_tournament_entries__users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "f_k_tournament_entries_tournaments_tournament_id",
                        column: x => x.tournament_id,
                        principalTable: "tournaments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_achievements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    achievement_id = table.Column<Guid>(type: "uuid", nullable: false),
                    current_progress = table.Column<int>(type: "integer", nullable: false),
                    is_unlocked = table.Column<bool>(type: "boolean", nullable: false),
                    reward_claimed = table.Column<bool>(type: "boolean", nullable: false),
                    unlocked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    claimed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_achievements", x => x.id);
                    table.ForeignKey(
                        name: "f_k_user_achievements_achievements_achievement_id",
                        column: x => x.achievement_id,
                        principalTable: "achievements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "f_k_user_achievements_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_battle_pass_progresses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    season_id = table.Column<Guid>(type: "uuid", nullable: false),
                    has_premium = table.Column<bool>(type: "boolean", nullable: false),
                    purchase_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    current_level = table.Column<int>(type: "integer", nullable: false),
                    current_xp = table.Column<int>(type: "integer", nullable: false),
                    total_xp_earned = table.Column<int>(type: "integer", nullable: false),
                    claimed_rewards = table.Column<string>(type: "jsonb", nullable: false),
                    claimed_free_rewards = table.Column<List<int>>(type: "integer[]", nullable: false),
                    claimed_premium_rewards = table.Column<List<int>>(type: "integer[]", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_battle_pass_progresses", x => x.id);
                    table.ForeignKey(
                        name: "f_k_user_battle_pass_progresses_battle_pass_seasons_season_id",
                        column: x => x.season_id,
                        principalTable: "battle_pass_seasons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "f_k_user_battle_pass_progresses_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_preferences",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    theme = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    sound_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    music_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    vibration_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    notifications_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    settings_json = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_preferences", x => x.id);
                    table.ForeignKey(
                        name: "f_k_user_preferences_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_premium_contents",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    premium_tier = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    subscription_active = table.Column<bool>(type: "boolean", nullable: false),
                    subscription_expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    battle_pass_active = table.Column<bool>(type: "boolean", nullable: false),
                    battle_pass_expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    battle_pass_tier = table.Column<int>(type: "integer", nullable: false),
                    owned_themes = table.Column<string>(type: "jsonb", nullable: false),
                    owned_powerups = table.Column<string>(type: "jsonb", nullable: false),
                    owned_cosmetics = table.Column<string>(type: "jsonb", nullable: false),
                    tournament_entries = table.Column<Dictionary<string, int>>(type: "jsonb", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_premium_contents", x => x.id);
                    table.ForeignKey(
                        name: "f_k_user_premium_contents_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "game_replays",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    score_id = table.Column<Guid>(type: "uuid", nullable: true),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_public = table.Column<bool>(type: "boolean", nullable: false),
                    replay_data = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: false),
                    final_score = table.Column<int>(type: "integer", nullable: false),
                    duration_seconds = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_replays", x => x.id);
                    table.ForeignKey(
                        name: "f_k_game_replays__scores_score_id",
                        column: x => x.score_id,
                        principalTable: "scores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "f_k_game_replays__users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_achievements_achievement_id",
                table: "achievements",
                column: "achievement_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_achievements_category",
                table: "achievements",
                column: "category");

            migrationBuilder.CreateIndex(
                name: "i_x_achievements_is_active",
                table: "achievements",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "i_x_battle_pass_seasons_is_active",
                table: "battle_pass_seasons",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "i_x_battle_pass_seasons_season_id",
                table: "battle_pass_seasons",
                column: "season_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_battle_pass_seasons_start_date_end_date",
                table: "battle_pass_seasons",
                columns: new[] { "start_date", "end_date" });

            migrationBuilder.CreateIndex(
                name: "i_x_fcm_tokens_token",
                table: "fcm_tokens",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_fcm_tokens_user_id",
                table: "fcm_tokens",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "i_x_friendships_friend_id_status",
                table: "friendships",
                columns: new[] { "friend_id", "status" });

            migrationBuilder.CreateIndex(
                name: "i_x_friendships_user_id_friend_id",
                table: "friendships",
                columns: new[] { "user_id", "friend_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_friendships_user_id_status",
                table: "friendships",
                columns: new[] { "user_id", "status" });

            migrationBuilder.CreateIndex(
                name: "i_x_game_replays_created_at",
                table: "game_replays",
                column: "created_at",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "i_x_game_replays_is_public",
                table: "game_replays",
                column: "is_public");

            migrationBuilder.CreateIndex(
                name: "i_x_game_replays_score_id",
                table: "game_replays",
                column: "score_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_game_replays_user_id",
                table: "game_replays",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "i_x_multiplayer_games_created_at",
                table: "multiplayer_games",
                column: "created_at",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "i_x_multiplayer_games_game_id",
                table: "multiplayer_games",
                column: "game_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_multiplayer_games_room_code",
                table: "multiplayer_games",
                column: "room_code");

            migrationBuilder.CreateIndex(
                name: "i_x_multiplayer_games_status",
                table: "multiplayer_games",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "i_x_multiplayer_players_game_id",
                table: "multiplayer_players",
                column: "game_id");

            migrationBuilder.CreateIndex(
                name: "i_x_multiplayer_players_game_id_user_id",
                table: "multiplayer_players",
                columns: new[] { "game_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_multiplayer_players_user_id",
                table: "multiplayer_players",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "i_x_notification_histories_sent_at",
                table: "notification_histories",
                column: "sent_at",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "i_x_notification_histories_type",
                table: "notification_histories",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "i_x_purchases_created_at",
                table: "purchases",
                column: "created_at",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "i_x_purchases_is_verified",
                table: "purchases",
                column: "is_verified");

            migrationBuilder.CreateIndex(
                name: "i_x_purchases_product_id",
                table: "purchases",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "i_x_purchases_transaction_id",
                table: "purchases",
                column: "transaction_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_purchases_user_id",
                table: "purchases",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "i_x_scheduled_jobs_job_id",
                table: "scheduled_jobs",
                column: "job_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_scheduled_jobs_next_run_time",
                table: "scheduled_jobs",
                column: "next_run_time");

            migrationBuilder.CreateIndex(
                name: "i_x_scheduled_jobs_status",
                table: "scheduled_jobs",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "i_x_scores_created_at",
                table: "scores",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "i_x_scores_game_mode_difficulty",
                table: "scores",
                columns: new[] { "game_mode", "difficulty" });

            migrationBuilder.CreateIndex(
                name: "i_x_scores_idempotency_key",
                table: "scores",
                column: "idempotency_key",
                unique: true,
                filter: "idempotency_key IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "i_x_scores_score_value",
                table: "scores",
                column: "score_value",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "i_x_scores_user_id_created_at",
                table: "scores",
                columns: new[] { "user_id", "created_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "i_x_tournament_entries_tournament_id_best_score",
                table: "tournament_entries",
                columns: new[] { "tournament_id", "best_score" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "i_x_tournament_entries_tournament_id_user_id",
                table: "tournament_entries",
                columns: new[] { "tournament_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_tournament_entries_user_id",
                table: "tournament_entries",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "i_x_tournaments_end_date",
                table: "tournaments",
                column: "end_date");

            migrationBuilder.CreateIndex(
                name: "i_x_tournaments_start_date",
                table: "tournaments",
                column: "start_date");

            migrationBuilder.CreateIndex(
                name: "i_x_tournaments_status",
                table: "tournaments",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "i_x_tournaments_status_start_date",
                table: "tournaments",
                columns: new[] { "status", "start_date" });

            migrationBuilder.CreateIndex(
                name: "i_x_tournaments_tournament_id",
                table: "tournaments",
                column: "tournament_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_user_achievements_achievement_id",
                table: "user_achievements",
                column: "achievement_id");

            migrationBuilder.CreateIndex(
                name: "i_x_user_achievements_is_unlocked",
                table: "user_achievements",
                column: "is_unlocked");

            migrationBuilder.CreateIndex(
                name: "i_x_user_achievements_user_id",
                table: "user_achievements",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "i_x_user_achievements_user_id_achievement_id",
                table: "user_achievements",
                columns: new[] { "user_id", "achievement_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_user_battle_pass_progresses_has_premium",
                table: "user_battle_pass_progresses",
                column: "has_premium");

            migrationBuilder.CreateIndex(
                name: "i_x_user_battle_pass_progresses_season_id",
                table: "user_battle_pass_progresses",
                column: "season_id");

            migrationBuilder.CreateIndex(
                name: "i_x_user_battle_pass_progresses_user_id",
                table: "user_battle_pass_progresses",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "i_x_user_battle_pass_progresses_user_id_season_id",
                table: "user_battle_pass_progresses",
                columns: new[] { "user_id", "season_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_user_preferences_user_id",
                table: "user_preferences",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_user_premium_contents_subscription_active",
                table: "user_premium_contents",
                column: "subscription_active");

            migrationBuilder.CreateIndex(
                name: "i_x_user_premium_contents_user_id",
                table: "user_premium_contents",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_users_email",
                table: "users",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "i_x_users_firebase_uid",
                table: "users",
                column: "firebase_uid",
                unique: true,
                filter: "firebase_uid IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "i_x_users_high_score",
                table: "users",
                column: "high_score",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "i_x_users_last_seen",
                table: "users",
                column: "last_seen");

            migrationBuilder.CreateIndex(
                name: "i_x_users_level",
                table: "users",
                column: "level");

            migrationBuilder.CreateIndex(
                name: "i_x_users_status",
                table: "users",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "i_x_users_username",
                table: "users",
                column: "username",
                unique: true,
                filter: "username IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fcm_tokens");

            migrationBuilder.DropTable(
                name: "friendships");

            migrationBuilder.DropTable(
                name: "game_replays");

            migrationBuilder.DropTable(
                name: "multiplayer_players");

            migrationBuilder.DropTable(
                name: "notification_histories");

            migrationBuilder.DropTable(
                name: "purchases");

            migrationBuilder.DropTable(
                name: "scheduled_jobs");

            migrationBuilder.DropTable(
                name: "tournament_entries");

            migrationBuilder.DropTable(
                name: "user_achievements");

            migrationBuilder.DropTable(
                name: "user_battle_pass_progresses");

            migrationBuilder.DropTable(
                name: "user_preferences");

            migrationBuilder.DropTable(
                name: "user_premium_contents");

            migrationBuilder.DropTable(
                name: "scores");

            migrationBuilder.DropTable(
                name: "multiplayer_games");

            migrationBuilder.DropTable(
                name: "tournaments");

            migrationBuilder.DropTable(
                name: "achievements");

            migrationBuilder.DropTable(
                name: "battle_pass_seasons");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
