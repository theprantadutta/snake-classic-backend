using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SnakeClassic.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchmakingAndReconnection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "connection_id",
                table: "multiplayer_players",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "disconnected_at",
                table: "multiplayer_players",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "eliminated_at",
                table: "multiplayer_players",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "elimination_rank",
                table: "multiplayer_players",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "matchmaking_queues",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    mode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    desired_players = table.Column<int>(type: "integer", nullable: false),
                    queued_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_matched = table.Column<bool>(type: "boolean", nullable: false),
                    matched_game_id = table.Column<Guid>(type: "uuid", nullable: true),
                    connection_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_matchmaking_queues", x => x.id);
                    table.ForeignKey(
                        name: "f_k_matchmaking_queues__users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_matchmaking_queues_mode_desired_players_is_matched_queued_at",
                table: "matchmaking_queues",
                columns: new[] { "mode", "desired_players", "is_matched", "queued_at" });

            migrationBuilder.CreateIndex(
                name: "i_x_matchmaking_queues_user_id",
                table: "matchmaking_queues",
                column: "user_id",
                unique: true,
                filter: "is_matched = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "matchmaking_queues");

            migrationBuilder.DropColumn(
                name: "connection_id",
                table: "multiplayer_players");

            migrationBuilder.DropColumn(
                name: "disconnected_at",
                table: "multiplayer_players");

            migrationBuilder.DropColumn(
                name: "eliminated_at",
                table: "multiplayer_players");

            migrationBuilder.DropColumn(
                name: "elimination_rank",
                table: "multiplayer_players");
        }
    }
}
