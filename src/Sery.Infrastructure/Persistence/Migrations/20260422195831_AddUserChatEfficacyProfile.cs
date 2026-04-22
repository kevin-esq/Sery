using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sery.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserChatEfficacyProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "user_chat_efficacy_profiles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreferredConversationMode = table.Column<string>(type: "text", nullable: false),
                    PreferredResponseLength = table.Column<string>(type: "text", nullable: false),
                    PreferredQuestionStyle = table.Column<string>(type: "text", nullable: false),
                    PreferredActionStyle = table.Column<string>(type: "text", nullable: false),
                    PreferredPacing = table.Column<string>(type: "text", nullable: false),
                    SuccessfulStrategiesSummary = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_chat_efficacy_profiles", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_user_chat_efficacy_profiles_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_chat_efficacy_profiles");
        }
    }
}
