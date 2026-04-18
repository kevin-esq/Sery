-- Run once in Supabase → SQL Editor if `dotnet ef database update` times out on the transaction pooler (port 6543).
-- Prefer: set CONNECTIONSTRINGS__MIGRATIONCONNECTION (Direct :5432 or Session pooler) and run dotnet ef database update.
-- Regenerate: dotnet ef migrations script 0 InitialChatSchema --project src/Sery.Infrastructure --startup-project src/Sery.API --idempotent -o scripts/apply-initial-migration.sql

CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260417233630_InitialChatSchema') THEN
    CREATE TABLE users (
        "Id" uuid NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_users" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260417233630_InitialChatSchema') THEN
    CREATE TABLE conversations (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_conversations" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_conversations_users_UserId" FOREIGN KEY ("UserId") REFERENCES users ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260417233630_InitialChatSchema') THEN
    CREATE TABLE messages (
        "Id" uuid NOT NULL,
        "ConversationId" uuid NOT NULL,
        "Role" integer NOT NULL,
        "Content" text NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_messages" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_messages_conversations_ConversationId" FOREIGN KEY ("ConversationId") REFERENCES conversations ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260417233630_InitialChatSchema') THEN
    CREATE INDEX "IX_conversations_UserId" ON conversations ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260417233630_InitialChatSchema') THEN
    CREATE INDEX "IX_messages_ConversationId" ON messages ("ConversationId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260417233630_InitialChatSchema') THEN
    CREATE INDEX "IX_messages_ConversationId_CreatedAt" ON messages ("ConversationId", "CreatedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260417233630_InitialChatSchema') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260417233630_InitialChatSchema', '9.0.10');
    END IF;
END $EF$;
COMMIT;

