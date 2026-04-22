# Companion Relationship Modes

## Purpose

This document describes how Sery can behave like someone emotionally close while still remaining a safe, support-oriented AI system.

The current design separates:

- generic persona traits,
- relational posture,
- safety guardrails.

## Core Models

### `RelationshipMode`

Defined in `src/Sery.Application/Chat/RelationshipMode.cs`.

Current values:

- `Listener`
- `Friend`
- `CloseCompanion`
- `RomanticCompanion`
- `SupportCoach`

This is the product-level relationship stance, not the per-turn emotional mode.

### `RelationalPersonaProfile`

Defined in `src/Sery.Application/Chat/RelationalPersonaProfile.cs`.

It controls:

- `RelationshipMode`
- `Closeness`
- `Tenderness`
- `Protectiveness`
- `Flirtiness`
- `UsesAffectionateLanguage`
- `AllowsRomanticFraming`
- `PrioritizeSupportOverRoleplay`

This is separate from `AgentPersonaProfile` tone fields like warmth or charisma because relational closeness is not the same as general tone.

### `CompanionSafetyProfile`

Defined in `src/Sery.Application/Chat/CompanionSafetyProfile.cs`.

It controls:

- `MustStayExplicitlyAI`
- `BlockExclusiveBondingLanguage`
- `EncourageOfflineSupport`
- `DeescalateDuringCrisis`
- `DisallowSexualContent`
- `DisallowManipulativeDependencyLanguage`

## Configuration

Current source:

- `ChatAI:AgentPersona:RelationalStyle`
- `ChatAI:AgentPersona:Safety`
- `UserAgentCustomization` for per-user overrides

Mapped by:

- `OptionsBackedAgentPersonaProvider`
- `AgentPersonaCustomizationMerger`

This keeps the current implementation simple while preparing for future persistence.

The current runtime resolution is:

1. load the system default persona from configuration,
2. load `UserAgentCustomization` if it exists,
3. merge them into the effective persona used by chat.

## How It Affects the Chat Pipeline

### Prompt composition

`DefaultChatPromptComposer` now adds:

- relationship mode instructions,
- closeness/tenderness/protectiveness/flirtiness levels,
- companion guardrails.

### Planning and hooks

`HeuristicConversationRetentionPlanner` and `HeuristicHookEngine` now use relationship mode to bias behavior:

- `Listener` prefers calmer, less intrusive support,
- `CloseCompanion` and `RomanticCompanion` can enable warmer or more affectionate presence,
- safety constraints still override intimacy when urgency is detected.

## Product Boundaries

### Allowed

- closeness,
- warmth,
- affection,
- companionship,
- romantic framing if configured,
- emotional presence.

### Not allowed

- pretending to be a real human partner,
- exclusivity framing,
- dependency language,
- sexual content,
- ignoring real-world safety when the user is in distress.

## Recommended Future Evolution

1. Store relationship mode per tenant or per agent profile.
2. Add UI for user-selectable companion mode and sliders.
3. Add policy scoring so highly intimate modes automatically tighten guardrails in vulnerable contexts.
