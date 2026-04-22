# Chat Retention and Persona

## Purpose

This document explains the current retention-oriented chat layer:

- conversation modes
- configurable agent persona
- prompt hooks
- adaptive turn shaping
- turn analysis
- adaptive pacing
- efficacy memory
- relationship modes
- companion guardrails

The implementation is intentionally heuristic-first. It creates a stable foundation before introducing analytics-driven optimization.

## Core pieces

### `ConversationMode`

Defined in `src/Sery.Application/Chat/ConversationMode.cs`.

Current modes:

- `Warm`
- `Direct`
- `Reflective`
- `Playful`
- `Grounding`
- `Deep`
- `CrisisSafe`

Each mode represents the desired emotional and interaction posture for a single turn, not a permanent personality.

### `AgentPersonaProfile`

Defined in `src/Sery.Application/Chat/AgentPersonaProfile.cs`.

This is the neutral model used by the chat layer to describe how the agent should feel and act:

- identity presentation
- warmth
- directness
- sincerity
- charisma
- playfulness
- reflection
- proactivity
- emotional expressiveness
- response length preference

Current source: `ChatAI:AgentPersona` from app configuration, mapped through `OptionsBackedAgentPersonaProvider`.

It now includes:

- generic tone traits,
- `RelationalPersonaProfile`,
- `CompanionSafetyProfile`.

### `AgentAdaptationProfile`

Defined in `src/Sery.Application/Chat/AgentAdaptationProfile.cs`.

Controls whether the agent mirrors:

- user energy
- user formality
- user verbosity

It also keeps space for stronger future adaptation controls.

### `IConversationRetentionPlanner`

Current implementation: `HeuristicConversationRetentionPlanner`.

Responsibility:

- inspect the user turn
- select a `ConversationMode`
- define turn-level intent:
  - opening style
  - relational goal
  - tempo
  - closing style

### `IHookEngine`

Current implementation: `HeuristicHookEngine`.

Responsibility:

- suggest lightweight connection hooks for the turn
- examples:
  - precise observation
  - continuity callback
  - inner contrast
  - micro next step
  - calibrated question
  - magnetic phrasing

Hooks are instructions for the model, not persisted artifacts.

### `ITurnAnalyzer`

Current implementation: `HeuristicTurnAnalyzer`.

Responsibility:

- classify the current turn into structured signals,
- detect emotional intensity, vulnerability, urgency, and user need,
- provide typed turn metadata that other layers can consume.

### `IAdaptivePacingPolicy`

Current implementation: `HeuristicAdaptivePacingPolicy`.

Responsibility:

- decide response length,
- decide tempo and question budget,
- decide whether the turn should use denser or lighter structure,
- keep pacing separate from pure emotional analysis.

### `IChatEfficacyProfiler`

Current implementation: `HeuristicChatEfficacyProfiler`.

Responsibility:

- infer which response postures seem to fit the user better over time,
- build a persisted `UserChatEfficacyProfile`,
- keep efficacy memory separate from emotional memory and conversation summaries.

### `IChatPromptComposer`

Current implementation: `DefaultChatPromptComposer`.

Responsibility:

- assemble:
  - base system prompt
  - persona profile
  - turn analysis
  - adaptation instructions
  - pacing instructions
  - retention plan
  - hooks
  - emotional memory
  - efficacy memory
  - compressed conversation memory

This keeps `ChatMessageService` focused on orchestration, persistence, and streaming.

## Turn flow

1. `ChatMessageService` loads conversation context, emotional memory, and efficacy memory.
2. The user message is persisted.
3. `ITurnAnalyzer` classifies the current turn.
4. `IAdaptivePacingPolicy` builds the pacing plan.
5. `IConversationRetentionPlanner` selects the conversation mode and relational strategy.
6. `IHookEngine` adds turn-level hooks.
7. `IChatPromptComposer` builds the prompt from prepared strategy objects.
8. The model streams the answer.
9. Conversation summary, emotional memory, and efficacy memory are refreshed after the assistant response is persisted.

## Product intent

The current retention layer is designed to increase:

- felt continuity
- emotional precision
- response distinctiveness
- willingness to continue the conversation
- response pacing that fits the moment
- cross-conversation learning about what style helps the user engage

It must not optimize for:

- romantic dependency
- exclusivity cues
- manipulative emotional escalation

## Next evolution

Recommended next steps:

1. Persist `AgentPersonaProfile` by tenant or agent.
2. Add `UserStyleProfile` for user-specific tone preferences.
3. Add explicit scoring for response quality and continuation rate.
4. Replace part of the heuristic analyzer and efficacy profiler with analytics-backed selection.
5. Introduce safety-aware overrides for sensitive emotional states.
