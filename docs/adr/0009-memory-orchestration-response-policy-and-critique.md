# 0009. Memory Orchestration, Response Policy, and Response Critique

Date: 2026-04-22

## Status

Accepted

## Context

The previous chat pipeline already had:

- turn analysis,
- adaptive pacing,
- retention planning,
- emotional memory,
- efficacy memory,
- configurable agent persona.

That was enough to improve warmth and continuity, but it was still too dependent on:

- raw history ordering,
- prompt-only continuity,
- one-shot generation quality,
- literal text patterns for closure detection.

This created concrete product problems:

- the assistant could reopen a topic that the user had already closed,
- the assistant could ask a stale follow-up that no longer fit the latest turn,
- noisy user text such as `ya se me calmo grasias` was harder to interpret reliably,
- memory was being used, but not with an explicit read/write policy,
- the system had no post-generation quality gate for continuity failures.

For a companion-style product, these failures are not minor. They directly break the sense of being understood.

## Decision

We introduced three explicit layers into the chat pipeline:

1. `IMemoryOrchestrator`
2. `IResponsePolicyEngine`
3. `IResponseCritic`

We also formalized `ITurnStateInterpreter` as the source of structured conversational state for closure-sensitive behavior.

## Architecture

### 1. Turn interpretation

`ITurnStateInterpreter` produces a `TurnStateInterpretation` that is separate from `TurnAnalysis`.

`TurnAnalysis` answers:

- what the user is expressing,
- how intense the emotion is,
- what kind of help seems needed.

`TurnStateInterpretation` answers:

- whether the user seems resolved,
- whether the user is closing the conversation,
- whether reopening should be avoided,
- whether a topic shift is likely,
- whether light closure is preferred.

This distinction is important. A user can still be emotional while also clearly closing the thread.

### 2. Memory orchestration

`IMemoryOrchestrator` separates memory usage into:

- `MemoryReadPlan`
- `MemoryWritePlan`

`MemoryReadPlan` controls what context is loaded into the prompt:

- recent history window,
- conversation summary inclusion,
- emotional memory inclusion,
- efficacy memory inclusion,
- focus instruction.

`MemoryWritePlan` controls what should be refreshed after the turn:

- conversation summary,
- emotional memory,
- efficacy profile,
- preference signal capture.

This is intentionally closer to how serious conversational systems treat memory. Retrieval and persistence are different decisions and should not be collapsed into one implicit behavior.

### 3. Response policy

`IResponsePolicyEngine` translates interpreted turn state into final response behavior.

This layer can override the initial retention plan and pacing plan when continuity requires it.

Examples:

- if the user is closing, force short length and avoid follow-up,
- if the user likely changed topic, keep continuity without dragging old emotional framing forward,
- if reopening risk is high, bias toward closure and acknowledgement.

This keeps the prompt composer simple. The composer assembles the final prompt; it does not decide policy.

### 4. Response critique

`IResponseCritic` reviews the assistant draft before it is emitted.

The current implementation uses an AI-backed critic with heuristic fallback.

It can rewrite when the draft:

- reopens a resolved topic,
- ignores the latest user message,
- repeats a stale support template,
- sounds melodramatic,
- asks a no-longer-relevant follow-up.

This introduces a quality gate between generation and delivery.

## Why this design

### Clean Code / SOLID

- `ChatMessageService` remains an orchestrator.
- `DefaultChatPromptComposer` remains a composer, not a policy engine.
- memory decisions, response decisions, and critique decisions are separated.
- each layer can be replaced independently.

This follows SRP more closely than pushing all continuity logic into one prompt or one service method.

### Product fit

Sery is not only an assistant. It is also a companion-style system that must feel:

- coherent,
- attentive,
- emotionally accurate,
- non-repetitive,
- adaptable to messy real-world user writing.

That requires more than good prompts. It requires explicit control over:

- what memory is read,
- what memory is written,
- when the model should close rather than continue,
- when a generated answer should be rejected or softened.

### Reliability

Relying on one-shot generation alone makes continuity too fragile.

The new pipeline is more robust because:

- interpretation is explicit,
- memory use is explicit,
- response policy is explicit,
- output quality has a second pass.

## Consequences

### Positive

- better handling of closure and resolved turns,
- less repetition,
- less prompt-only coupling,
- clearer extension points for telemetry and learning,
- better support for noisy user input.

### Negative

- more orchestration complexity,
- more components to test,
- current SSE behavior is no longer true token-by-token provider streaming because critique happens before emission.

## Streaming tradeoff

The current design buffers the provider draft, critiques it, and then emits final chunks.

We accept this tradeoff for now because continuity failures were more damaging than the loss of raw token streaming.

If product requirements later demand true live streaming, the correct next step is not to remove critique entirely, but to introduce two paths:

1. fast-path streaming for low-risk turns,
2. buffered critique path for closure-sensitive or high-risk turns.

## Implementation notes

Current implementations:

- `AIBackedTurnStateInterpreter`
- `HeuristicMemoryOrchestrator`
- `HeuristicResponsePolicyEngine`
- `AIBackedResponseCritic`

Supporting utility:

- `TextHeuristics`

Main orchestration point:

- `ChatMessageService`

Main prompt assembly point:

- `DefaultChatPromptComposer`

## Follow-up work

1. Add telemetry for reopen failures and critique rewrite rate.
2. Add `UserStyleProfile` so response policy can use learned preferences.
3. Add moderation-aware policy overrides before critique.
4. Split critique into fast heuristic pass and optional AI deep pass.
5. Introduce dual streaming behavior for low-risk vs closure-sensitive turns.
