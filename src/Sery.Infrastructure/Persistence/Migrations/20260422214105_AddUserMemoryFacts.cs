using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sery.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserMemoryFacts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "user_memory_facts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceConversationId = table.Column<Guid>(type: "uuid", nullable: true),
                    Category = table.Column<string>(type: "text", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    NormalizedKey = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastSeenAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_memory_facts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_memory_facts_conversations_SourceConversationId",
                        column: x => x.SourceConversationId,
                        principalTable: "conversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_user_memory_facts_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_user_memory_facts_SourceConversationId",
                table: "user_memory_facts",
                column: "SourceConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_user_memory_facts_UserId",
                table: "user_memory_facts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_user_memory_facts_UserId_LastSeenAt",
                table: "user_memory_facts",
                columns: new[] { "UserId", "LastSeenAt" });

            migrationBuilder.CreateIndex(
                name: "IX_user_memory_facts_UserId_NormalizedKey",
                table: "user_memory_facts",
                columns: new[] { "UserId", "NormalizedKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_memory_facts");
        }
    }
}
