using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Sery.Application.AgentCustomization;
using Sery.Application.Chat;
using Sery.Application.Conversations;
using Sery.Domain.Entities;

namespace Sery.API.IntegrationTests;

/// <summary>
/// Registers a stub chat service so chat endpoints can be exercised without external services.
/// </summary>
public sealed class ChatWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IChatMessageService>();
            services.RemoveAll<IConversationService>();
            services.RemoveAll<IAgentCustomizationService>();
            services.AddScoped<IChatMessageService, StubChatMessageService>();
            services.AddScoped<IConversationService, StubConversationService>();
            services.AddScoped<IAgentCustomizationService, StubAgentCustomizationService>();
            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });
        });
    }

    private sealed class StubConversationService : IConversationService
    {
        private static readonly Guid ExistingConversationId = Guid.Parse("1c9a18d3-18b3-4f45-8bc3-bcb6b9fe7f38");

        public Task<IReadOnlyList<ConversationListItemDto>> GetConversationsAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyList<ConversationListItemDto> conversations =
            [
                new ConversationListItemDto(
                    ExistingConversationId,
                    "Anxiety before tomorrow's meeting",
                    "The conversation focuses on work pressure. The user's recent tone has been mostly anxious. Helpful responses so far use grounding and breathing cues.",
                    new DateTime(2026, 4, 21, 20, 56, 52, DateTimeKind.Utc),
                    new DateTime(2026, 4, 21, 21, 1, 18, DateTimeKind.Utc),
                    new DateTime(2026, 4, 21, 21, 1, 18, DateTimeKind.Utc),
                    "Respira conmigo. Vamos paso a paso.",
                    14,
                    false,
                    true)
            ];

            return Task.FromResult(conversations);
        }

        public Task<ConversationDetailsDto?> GetConversationAsync(
            Guid userId,
            Guid conversationId,
            CancellationToken cancellationToken = default)
        {
            ConversationDetailsDto? conversation = conversationId == ExistingConversationId
                ? new ConversationDetailsDto(
                    conversationId,
                    "Anxiety before tomorrow's meeting",
                    "The conversation focuses on work pressure. The user's recent tone has been mostly anxious. Helpful responses so far use grounding and breathing cues.",
                    new DateTime(2026, 4, 21, 20, 56, 52, DateTimeKind.Utc),
                    new DateTime(2026, 4, 21, 21, 1, 18, DateTimeKind.Utc),
                    new DateTime(2026, 4, 21, 21, 1, 18, DateTimeKind.Utc),
                    "Respira conmigo. Vamos paso a paso.",
                    14,
                    false,
                    true)
                : null;

            return Task.FromResult(conversation);
        }

        public Task<IReadOnlyList<ConversationMessageDto>> GetMessagesAsync(
            Guid userId,
            Guid conversationId,
            int skip,
            int take,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyList<ConversationMessageDto> messages =
            [
                new ConversationMessageDto(
                    Guid.Parse("3c1b26d0-84f7-40f3-a3a8-00a517d9bf03"),
                    MessageRole.User,
                    "Hola Sery, hoy me siento nervioso.",
                    new DateTime(2026, 4, 21, 20, 57, 0, DateTimeKind.Utc))
            ];

            return Task.FromResult(messages);
        }

        public Task<bool> DeleteConversationAsync(
            Guid userId,
            Guid conversationId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(conversationId == ExistingConversationId);
        }

        public Task<ConversationDetailsDto?> UpdateMetadataAsync(
            Guid userId,
            Guid conversationId,
            ConversationMetadataUpdateDto update,
            CancellationToken cancellationToken = default)
        {
            ConversationDetailsDto? conversation = conversationId == ExistingConversationId
                ? new ConversationDetailsDto(
                    conversationId,
                    update.Title ?? "Anxiety before tomorrow's meeting",
                    "The conversation focuses on work pressure. The user's recent tone has been mostly anxious. Helpful responses so far use grounding and breathing cues.",
                    new DateTime(2026, 4, 21, 20, 56, 52, DateTimeKind.Utc),
                    new DateTime(2026, 4, 21, 21, 5, 0, DateTimeKind.Utc),
                    new DateTime(2026, 4, 21, 21, 1, 18, DateTimeKind.Utc),
                    "Respira conmigo. Vamos paso a paso.",
                    14,
                    update.IsArchived ?? false,
                    update.IsPinned ?? true)
                : null;

            return Task.FromResult(conversation);
        }
    }

    private sealed class StubAgentCustomizationService : IAgentCustomizationService
    {
        private AgentCustomizationDto current = new(
            "Sery",
            "neutral",
            "supportive",
            78,
            58,
            88,
            62,
            18,
            72,
            66,
            54,
            "medium",
            true,
            true,
            "Friend",
            72,
            68,
            56,
            0,
            false,
            false,
            true,
            false,
            null);

        public Task<AgentCustomizationDto> GetAsync(Guid userId, CancellationToken cancellationToken)
        {
            return Task.FromResult(current);
        }

        public Task<AgentCustomizationDto> UpdateAsync(Guid userId, UpdateAgentCustomizationCommand command, CancellationToken cancellationToken)
        {
            current = current with
            {
                IdentityPresentation = command.IdentityPresentation ?? current.IdentityPresentation,
                CoreDemeanor = command.CoreDemeanor ?? current.CoreDemeanor,
                Warmth = command.Warmth ?? current.Warmth,
                Directness = command.Directness ?? current.Directness,
                Sincerity = command.Sincerity ?? current.Sincerity,
                Charisma = command.Charisma ?? current.Charisma,
                Playfulness = command.Playfulness ?? current.Playfulness,
                Reflection = command.Reflection ?? current.Reflection,
                Proactivity = command.Proactivity ?? current.Proactivity,
                EmotionalExpressiveness = command.EmotionalExpressiveness ?? current.EmotionalExpressiveness,
                PreferredResponseLength = command.PreferredResponseLength ?? current.PreferredResponseLength,
                AskFollowUpQuestions = command.AskFollowUpQuestions ?? current.AskFollowUpQuestions,
                OfferActionSteps = command.OfferActionSteps ?? current.OfferActionSteps,
                RelationshipMode = command.RelationshipMode ?? current.RelationshipMode,
                Closeness = command.Closeness ?? current.Closeness,
                Tenderness = command.Tenderness ?? current.Tenderness,
                Protectiveness = command.Protectiveness ?? current.Protectiveness,
                Flirtiness = command.Flirtiness ?? current.Flirtiness,
                UsesAffectionateLanguage = command.UsesAffectionateLanguage ?? current.UsesAffectionateLanguage,
                AllowsRomanticFraming = command.AllowsRomanticFraming ?? current.AllowsRomanticFraming,
                PrioritizeSupportOverRoleplay = command.PrioritizeSupportOverRoleplay ?? current.PrioritizeSupportOverRoleplay,
                IsCustomized = true,
                UpdatedAt = DateTime.UtcNow
            };

            return Task.FromResult(current);
        }

        public Task ResetAsync(Guid userId, CancellationToken cancellationToken)
        {
            current = current with
            {
                IdentityPresentation = "neutral",
                CoreDemeanor = "supportive",
                Warmth = 78,
                RelationshipMode = "Friend",
                IsCustomized = false,
                UpdatedAt = null
            };

            return Task.CompletedTask;
        }
    }
}
