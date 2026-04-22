using Microsoft.EntityFrameworkCore;
using Sery.Domain.Entities;

namespace Sery.Infrastructure.Persistence;

public sealed class SeryDbContext(DbContextOptions<SeryDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();
    public DbSet<UserEmotionalMemory> UserEmotionalMemories => Set<UserEmotionalMemory>();
    public DbSet<UserChatEfficacyProfile> UserChatEfficacyProfiles => Set<UserChatEfficacyProfile>();
    public DbSet<UserAgentCustomization> UserAgentCustomizations => Set<UserAgentCustomization>();
    public DbSet<UserMemoryFact> UserMemoryFacts => Set<UserMemoryFact>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Email).IsRequired();
            entity.HasIndex(x => x.Email).IsUnique();
            entity.Property(x => x.PasswordHash).IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.TenantId);
        });

        modelBuilder.Entity<UserEmotionalMemory>(entity =>
        {
            entity.ToTable("user_emotional_memories");
            entity.HasKey(x => x.UserId);
            entity.Property(x => x.Summary).IsRequired();
            entity.Property(x => x.DominantEmotion).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();

            entity.HasOne(x => x.User)
                .WithOne(x => x.EmotionalMemory)
                .HasForeignKey<UserEmotionalMemory>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserChatEfficacyProfile>(entity =>
        {
            entity.ToTable("user_chat_efficacy_profiles");
            entity.HasKey(x => x.UserId);
            entity.Property(x => x.PreferredConversationMode).IsRequired();
            entity.Property(x => x.PreferredResponseLength).IsRequired();
            entity.Property(x => x.PreferredQuestionStyle).IsRequired();
            entity.Property(x => x.PreferredActionStyle).IsRequired();
            entity.Property(x => x.PreferredPacing).IsRequired();
            entity.Property(x => x.SuccessfulStrategiesSummary).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();

            entity.HasOne(x => x.User)
                .WithOne(x => x.ChatEfficacyProfile)
                .HasForeignKey<UserChatEfficacyProfile>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserAgentCustomization>(entity =>
        {
            entity.ToTable("user_agent_customizations");
            entity.HasKey(x => x.UserId);
            entity.Property(x => x.IdentityPresentation).IsRequired();
            entity.Property(x => x.CoreDemeanor).IsRequired();
            entity.Property(x => x.Warmth).IsRequired();
            entity.Property(x => x.Directness).IsRequired();
            entity.Property(x => x.Sincerity).IsRequired();
            entity.Property(x => x.Charisma).IsRequired();
            entity.Property(x => x.Playfulness).IsRequired();
            entity.Property(x => x.Reflection).IsRequired();
            entity.Property(x => x.Proactivity).IsRequired();
            entity.Property(x => x.EmotionalExpressiveness).IsRequired();
            entity.Property(x => x.PreferredResponseLength).IsRequired();
            entity.Property(x => x.AskFollowUpQuestions).IsRequired();
            entity.Property(x => x.OfferActionSteps).IsRequired();
            entity.Property(x => x.RelationshipMode).IsRequired();
            entity.Property(x => x.Closeness).IsRequired();
            entity.Property(x => x.Tenderness).IsRequired();
            entity.Property(x => x.Protectiveness).IsRequired();
            entity.Property(x => x.Flirtiness).IsRequired();
            entity.Property(x => x.UsesAffectionateLanguage).IsRequired();
            entity.Property(x => x.AllowsRomanticFraming).IsRequired();
            entity.Property(x => x.PrioritizeSupportOverRoleplay).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();

            entity.HasOne(x => x.User)
                .WithOne(x => x.AgentCustomization)
                .HasForeignKey<UserAgentCustomization>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserMemoryFact>(entity =>
        {
            entity.ToTable("user_memory_facts");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Category).IsRequired();
            entity.Property(x => x.Content).IsRequired();
            entity.Property(x => x.NormalizedKey).IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();
            entity.Property(x => x.LastSeenAt).IsRequired();
            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => new { x.UserId, x.NormalizedKey }).IsUnique();
            entity.HasIndex(x => new { x.UserId, x.LastSeenAt });

            entity.HasOne(x => x.User)
                .WithMany(x => x.MemoryFacts)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.SourceConversation)
                .WithMany()
                .HasForeignKey(x => x.SourceConversationId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<UserSession>(entity =>
        {
            entity.ToTable("user_sessions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.RefreshTokenHash).IsRequired();
            entity.HasIndex(x => x.RefreshTokenHash).IsUnique();
            entity.Property(x => x.ExpiresAt).IsRequired();
            entity.HasIndex(x => x.UserId);

            entity.HasOne(x => x.User)
                .WithMany(x => x.Sessions)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.ToTable("conversations");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).IsRequired();
            entity.Property(x => x.IsTitleGenerated).IsRequired();
            entity.Property(x => x.Summary).IsRequired();
            entity.Property(x => x.IsArchived).IsRequired();
            entity.Property(x => x.IsPinned).IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();
            entity.Property(x => x.LastMessageAt);
            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => new { x.UserId, x.IsPinned, x.LastMessageAt });

            entity.HasOne(x => x.User)
                .WithMany(x => x.Conversations)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.ToTable("messages");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Content).IsRequired();
            entity.Property(x => x.Role).IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.HasIndex(x => x.ConversationId);
            entity.HasIndex(x => new { x.ConversationId, x.CreatedAt });

            entity.HasOne(x => x.Conversation)
                .WithMany(x => x.Messages)
                .HasForeignKey(x => x.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
