using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SnakeClassic.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDailyLoginBonus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "daily_login_bonuses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    current_streak = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    last_claim_date = table.Column<DateOnly>(type: "date", nullable: true),
                    total_claims = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_daily_login_bonuses", x => x.id);
                    table.ForeignKey(
                        name: "f_k_daily_login_bonuses__users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_daily_login_bonuses_user_id",
                table: "daily_login_bonuses",
                column: "user_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "daily_login_bonuses");
        }
    }
}
