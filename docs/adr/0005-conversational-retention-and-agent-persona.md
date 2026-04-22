# 0005 - Conversational Retention Strategy and Configurable Agent Persona

- Status: Accepted
- Date: 2026-04-22
- Deciders: Sery Backend Team

## Context

The chat experience needed stronger continuity and emotional pull without drifting into manipulative or parasocially unsafe behavior. The existing chat implementation already had:

- authenticated user context
- conversation summaries
- emotional memory summaries
- a prompt composer with configurable persona traits

However, retention behavior was still too implicit. The system did not explicitly decide:

- how to adapt the conversation to the current turn
- what conversational mode to use
- what kind of opening/closing pattern should guide the answer
- what lightweight "hooks" should make the response feel memorable and connective

The product direction required a foundation that could later support:

- persona variants by tenant or agent
- user-specific tone preferences
- experimentation on retention and conversation quality

## Decision

- Introduce an explicit `ConversationMode` model in `Sery.Application.Chat`.
- Introduce neutral `AgentPersonaProfile` and `AgentAdaptationProfile` models in `Application`.
- Add `IConversationRetentionPlanner` to choose a turn-level conversation strategy.
- Add `IHookEngine` to produce natural connection hooks for the current turn.
- Add `IAgentPersonaProvider` so prompt composition can read persona/adaptation settings from a source that can evolve later.
- Keep the current source of persona configuration in `ChatAIOptions`, mapped through `OptionsBackedAgentPersonaProvider`.
- Keep `ChatMessageService` orchestration-only; prompt shaping remains delegated to `IChatPromptComposer`.
- Extend `DefaultChatPromptComposer` so every prompt contains:
  - base system prompt
  - explicit persona profile
  - adaptive guidance
  - turn-level retention plan
  - hook instructions
  - emotional and conversation memory

## Consequences

### Positive

- Retention behavior becomes testable and inspectable instead of being hidden in ad-hoc prompt text.
- Persona customization is prepared for future persistence in database or admin panels.
- The system can evolve mode selection and hooks independently from transport and persistence.
- The prompt becomes more consistent across turns and more intentional in how it builds rapport.

### Negative / Trade-offs

- Prompt generation now has more moving parts and requires discipline to avoid overcomplication.
- The current planner and hook engine are heuristic and not yet learned from behavioral analytics.
- The persona source is still config-backed, not tenant-backed or user-backed yet.

## Alternatives Considered

1. **Keep prompt logic directly inside `ChatMessageService`** - rejected because it would overcouple orchestration with conversational behavior.
2. **Drive adaptation only through raw prompt text and string concatenation** - rejected because retention rules would be hard to test and evolve safely.
3. **Wait until a full analytics system exists** - rejected because the chat already needed a better baseline architecture for retention and adaptability.
