using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sery.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddConversationMemoryAndManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "conversations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPinned",
                table: "conversations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsTitleGenerated",
                table: "conversations",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastMessageAt",
                table: "conversations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Summary",
                table: "conversations",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "conversations",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "conversations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.CreateTable(
                name: "user_emotional_memories",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Summary = table.Column<string>(type: "text", nullable: false),
                    DominantEmotion = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_emotional_memories", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_user_emotional_memories_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_conversations_UserId_IsPinned_LastMessageAt",
                table: "conversations",
                columns: new[] { "UserId", "IsPinned", "LastMessageAt" });

            migrationBuilder.Sql("""
                UPDATE conversations
                SET "UpdatedAt" = "CreatedAt",
                    "IsTitleGenerated" = TRUE;
                """);

            migrationBuilder.Sql("""
                UPDATE conversations AS c
                SET "LastMessageAt" = m."LastMessageAt"
                FROM (
                    SELECT "ConversationId", MAX("CreatedAt") AS "LastMessageAt"
                    FROM messages
                    GROUP BY "ConversationId"
                ) AS m
                WHERE c."Id" = m."ConversationId";
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_emotional_memories");

            migrationBuilder.DropIndex(
                name: "IX_conversations_UserId_IsPinned_LastMessageAt",
                table: "conversations");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "conversations");

            migrationBuilder.DropColumn(
                name: "IsPinned",
                table: "conversations");

            migrationBuilder.DropColumn(
                name: "IsTitleGenerated",
                table: "conversations");

            migrationBuilder.DropColumn(
                name: "LastMessageAt",
                table: "conversations");

            migrationBuilder.DropColumn(
                name: "Summary",
                table: "conversations");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "conversations");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "conversations");
        }
    }
}
