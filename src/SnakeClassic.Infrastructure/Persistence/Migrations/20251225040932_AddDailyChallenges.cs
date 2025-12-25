using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SnakeClassic.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDailyChallenges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "daily_challenges",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    challenge_date = table.Column<DateOnly>(type: "date", nullable: false),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    difficulty = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    target_value = table.Column<int>(type: "integer", nullable: false),
                    coin_reward = table.Column<int>(type: "integer", nullable: false),
                    xp_reward = table.Column<int>(type: "integer", nullable: false),
                    required_game_mode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_daily_challenges", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_daily_challenges",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    challenge_id = table.Column<Guid>(type: "uuid", nullable: false),
                    current_progress = table.Column<int>(type: "integer", nullable: false),
                    is_completed = table.Column<bool>(type: "boolean", nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    claimed_reward = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_daily_challenges", x => x.id);
                    table.ForeignKey(
                        name: "f_k_user_daily_challenges_daily_challenges_challenge_id",
                        column: x => x.challenge_id,
                        principalTable: "daily_challenges",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "f_k_user_daily_challenges_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_daily_challenges_challenge_date",
                table: "daily_challenges",
                column: "challenge_date");

            migrationBuilder.CreateIndex(
                name: "i_x_daily_challenges_challenge_date_difficulty",
                table: "daily_challenges",
                columns: new[] { "challenge_date", "difficulty" });

            migrationBuilder.CreateIndex(
                name: "i_x_user_daily_challenges_challenge_id",
                table: "user_daily_challenges",
                column: "challenge_id");

            migrationBuilder.CreateIndex(
                name: "i_x_user_daily_challenges_user_id",
                table: "user_daily_challenges",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "i_x_user_daily_challenges_user_id_challenge_id",
                table: "user_daily_challenges",
                columns: new[] { "user_id", "challenge_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_user_daily_challenges_user_id_is_completed_claimed_reward",
                table: "user_daily_challenges",
                columns: new[] { "user_id", "is_completed", "claimed_reward" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_daily_challenges");

            migrationBuilder.DropTable(
                name: "daily_challenges");
        }
    }
}
