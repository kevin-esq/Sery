# 0006 - Turn Analysis, Adaptive Pacing, and Efficacy Memory

- Status: Accepted
- Date: 2026-04-22
- Deciders: Sery Backend Team

## Context

The previous retention iteration introduced persona, conversation modes, and hook generation, but the chat pipeline still lacked three explicit decisions:

- how to classify the current turn beyond raw text heuristics embedded in prompts,
- how to adapt pacing and response density per turn,
- how to remember which response styles seem to work better for each user over time.

At the same time, the product direction requires a chat that feels:

- faster,
- more emotionally precise,
- more adaptive,
- more consistent across conversations,
- still safe and non-manipulative.

The existing state already included:

- `Conversation` summaries,
- `UserEmotionalMemory`,
- configurable `AgentPersonaProfile`,
- retention hooks and conversation modes.

That was not enough to support a stronger adaptive system, because emotional memory and conversation summary answer different questions than response efficacy:

- emotional memory answers: "what has the user been feeling?"
- conversation summary answers: "what has this conversation been about?"
- efficacy memory answers: "what response posture tends to help this user engage better?"

## Decision

- Introduce `ITurnAnalyzer` with a heuristic first implementation, `HeuristicTurnAnalyzer`.
- Introduce `IAdaptivePacingPolicy` with `HeuristicAdaptivePacingPolicy`.
- Introduce `IChatEfficacyProfiler` with `HeuristicChatEfficacyProfiler`.
- Extend `ConversationContext` so the orchestration layer receives:
  - conversation history,
  - preferred language,
  - emotional memory,
  - efficacy memory.
- Add `UserChatEfficacyProfile` as a persisted one-to-one entity associated with `User`.
- Refactor `ChatMessageService` so it orchestrates an explicit per-turn pipeline:
  1. load context,
  2. persist user message,
  3. analyze the turn,
  4. build pacing plan,
  5. build retention plan,
  6. build hooks,
  7. compose the final system prompt,
  8. stream assistant output,
  9. refresh conversation summary, emotional memory, and efficacy memory.
- Refactor `DefaultChatPromptComposer` so it no longer performs planning internally. It now consumes a fully prepared `ChatPromptContext`.

## Why Separate These Concerns

### Turn Analysis

`TurnAnalysis` captures the user turn as an explicit model:

- primary emotion,
- intensity,
- energy,
- formality,
- disclosure depth,
- user need,
- vulnerability and urgency signals.

This lets the system make decisions in typed application logic instead of hiding everything inside prompt text.

### Adaptive Pacing

Pacing is not the same thing as emotional classification. Two users may both be anxious, but one needs:

- a short grounding answer,

while another may tolerate:

- a medium-length, reflective response.

`AdaptivePacingPlan` therefore exists as a separate artifact. It controls:

- response length,
- tempo,
- backchannel visibility,
- question budget,
- paragraph usage,
- bullet preference,
- opening cadence,
- closing cadence.

### Efficacy Memory

Emotional memory should not be overloaded with stylistic conclusions. A user can remain anxious over time while still responding better to:

- direct responses,
- reflective questions,
- short answers,
- slower pacing.

Persisting `UserChatEfficacyProfile` separately keeps:

- emotional state memory,
- conversation memory,
- response strategy memory

as distinct concepts with single responsibilities.

## Consequences

### Positive

- Chat behavior is now inspectable at each stage of a turn.
- Prompt composition is simpler because it consumes prepared strategy objects.
- Future ML or analytics-backed replacements can target one stage at a time.
- The system is prepared for:
  - user style profiles,
  - experimentation,
  - tenant-specific agent behavior,
  - stronger moderation overrides.

### Negative / Trade-offs

- The orchestration path is more complex and introduces more collaborating services.
- The current analyzer, pacing policy, and efficacy profiler are heuristic rather than learned.
- Efficacy memory infers what "works" from recent conversational shape, not from explicit user ratings yet.
- More persisted state means more schema evolution and migration discipline.

## Alternatives Considered

1. **Store adaptation hints only in the conversation summary**
   Rejected because summary text becomes overloaded, unstructured, and difficult to test.

2. **Keep the prompt composer responsible for analyzing the user turn**
   Rejected because it hides application behavior inside infrastructure and violates separation of concerns.

3. **Wait for analytics before persisting efficacy memory**
   Rejected because the product already benefits from a structured place to accumulate heuristic strategy knowledge.

## Follow-up Work

1. Persist `AgentPersonaProfile` by tenant or agent.
2. Introduce explicit `UserStyleProfile`.
3. Add quality and continuation telemetry so efficacy memory can evolve from heuristic to evidence-based.
4. Add safety overrides that can force pacing and mode regardless of the default planner.
