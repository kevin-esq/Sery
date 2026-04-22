using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sery.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserAgentCustomization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "user_agent_customizations",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IdentityPresentation = table.Column<string>(type: "text", nullable: false),
                    CoreDemeanor = table.Column<string>(type: "text", nullable: false),
                    Warmth = table.Column<int>(type: "integer", nullable: false),
                    Directness = table.Column<int>(type: "integer", nullable: false),
                    Sincerity = table.Column<int>(type: "integer", nullable: false),
                    Charisma = table.Column<int>(type: "integer", nullable: false),
                    Playfulness = table.Column<int>(type: "integer", nullable: false),
                    Reflection = table.Column<int>(type: "integer", nullable: false),
                    Proactivity = table.Column<int>(type: "integer", nullable: false),
                    EmotionalExpressiveness = table.Column<int>(type: "integer", nullable: false),
                    PreferredResponseLength = table.Column<string>(type: "text", nullable: false),
                    AskFollowUpQuestions = table.Column<bool>(type: "boolean", nullable: false),
                    OfferActionSteps = table.Column<bool>(type: "boolean", nullable: false),
                    RelationshipMode = table.Column<string>(type: "text", nullable: false),
                    Closeness = table.Column<int>(type: "integer", nullable: false),
                    Tenderness = table.Column<int>(type: "integer", nullable: false),
                    Protectiveness = table.Column<int>(type: "integer", nullable: false),
                    Flirtiness = table.Column<int>(type: "integer", nullable: false),
                    UsesAffectionateLanguage = table.Column<bool>(type: "boolean", nullable: false),
                    AllowsRomanticFraming = table.Column<bool>(type: "boolean", nullable: false),
                    PrioritizeSupportOverRoleplay = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_agent_customizations", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_user_agent_customizations_users_UserId",
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
                name: "user_agent_customizations");
        }
    }
}
