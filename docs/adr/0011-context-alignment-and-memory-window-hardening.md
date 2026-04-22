# 0011 - Context Alignment and Memory Window Hardening

- Status: Accepted
- Date: 2026-04-22
- Deciders: Sery Backend Team

## Context

The chat pipeline introduced in ADRs 0005–0010 established a multi-stage architecture with:

- turn analysis,
- AI-backed turn state interpretation,
- memory orchestration,
- response policy,
- AI-backed response critique.

In production testing, users reported that the assistant "does not remember" previous messages and lacks conversational coherence. The root cause was not a missing feature, but a **context alignment failure** across pipeline stages.

### Specific failures identified

1. **Inconsistent context windows across stages.**
   Each AI-backed component saw a different slice of the conversation:

    | Component                      | History window |
    | ------------------------------ | -------------- |
    | `AIBackedTurnStateInterpreter` | 4 messages     |
    | `HeuristicMemoryOrchestrator`  | 4–8 messages   |
    | Main LLM generation            | 4–8 messages   |
    | `AIBackedResponseCritic`       | 4 messages     |

    This meant that the TSI could classify a turn as "resolved" or "topic shift" with only 4 messages of context, then the orchestrator would reduce the window further, and the critic would evaluate the draft with even less context than the LLM had.

2. **Double context truncation.**
   The database layer fetched at most 8 messages (`ConversationContextLimit = 8`), then the orchestrator applied a second `TakeLast(4–6)` cut. Conversations beyond ~10 messages lost coherence.

3. **Conversation summary gated on `TopicShiftLikely`.**
   The memory orchestrator excluded the conversation summary when the TSI marked `TopicShiftLikely = true`. This was counterproductive — the summary exists precisely to bridge context gaps during topic shifts.

4. **Low-confidence critic rewrites.**
   The `AIBackedResponseCritic` could rewrite the entire draft even when its own confidence was `"low"`. With only 4 messages of context, it frequently replaced contextually correct responses with generic or disconnected text.

5. **`GeminiChatAIService` set `HttpClient.BaseAddress` inside `GenerateStreamAsync`.**
   In .NET, `HttpClient.BaseAddress` cannot be reassigned after the first request. Since the same scoped `HttpClient` was used for all three AI calls per turn (TSI, main LLM, critic), the second call threw `InvalidOperationException` — causing every chat message to fall back to the hardcoded error response.

### Cascade effect

These failures compounded:

1. TSI misclassifies with insufficient context.
2. Orchestrator trusts the misclassification and reduces the window.
3. Summary is excluded.
4. LLM generates with a narrow view.
5. Critic rewrites with even less context.
6. User receives a disconnected response.

## Decision

### 1. Unified minimum context across the pipeline

All AI-backed components now see the same minimum history depth:

| Component                      | Before | After |
| ------------------------------ | ------ | ----- |
| `AIBackedTurnStateInterpreter` | 4      | 12    |
| `AIBackedResponseCritic`       | 4      | 12    |

### 2. Wider database context window

```
ConversationContextLimit: 8 → 30
```

The database now provides enough history for the orchestrator to make meaningful decisions without being starved of data.

### 3. Increased orchestrator history windows

| Scenario                | Before | After |
| ----------------------- | ------ | ----- |
| Closing / light closure | 4      | 10    |
| Action-oriented         | 5      | 12    |
| Default                 | 6      | 16    |
| Deep / reflective       | 8      | 20    |

### 4. Conversation summary always included

```csharp
// Before
bool includeConversationSummary = context.Conversation is not null && !turnState.TopicShiftLikely;

// After
bool includeConversationSummary = context.Conversation is not null;
```

The summary is the compressed bridge that maintains continuity across the full conversation arc. Gating it on a low-confidence signal was counterproductive.

### 5. Critic soft mode

The `AIBackedResponseCritic` no longer rewrites when `confidence == "low"`. Low-confidence rewrites destroyed contextually correct responses more often than they fixed real problems.

```csharp
if (shouldRewrite && confidence == "low")
{
    shouldRewrite = false;
    finalResponse = SanitizeFinalResponse(context, userMessage, draftResponse, raw);
}
```

### 6. `GeminiChatAIService` initialization fix

Moved `HttpClient.BaseAddress` and `x-goog-api-key` header setup from `GenerateStreamAsync()` to the constructor, matching the `OllamaChatAIService` pattern.

## Files changed

| File                                                     | Change                                                                      |
| -------------------------------------------------------- | --------------------------------------------------------------------------- |
| `Sery.Application/Chat/ChatMessageService.cs`            | `ConversationContextLimit` 8 → 30                                           |
| `Sery.Application/Chat/HeuristicMemoryOrchestrator.cs`   | History windows widened; summary always included; relevant limits increased |
| `Sery.Infrastructure/AI/AIBackedTurnStateInterpreter.cs` | `TakeLast(4)` → `TakeLast(12)`                                              |
| `Sery.Infrastructure/AI/AIBackedResponseCritic.cs`       | `TakeLast(4)` → `TakeLast(12)`; soft mode for low-confidence rewrites       |
| `Sery.Infrastructure/AI/Gemini/GeminiChatAIService.cs`   | `BaseAddress` and API key moved to constructor                              |

## Consequences

### Positive

- All pipeline stages now share a consistent view of the conversation.
- Conversations with 20+ messages maintain coherence.
- The summary is no longer dropped on noisy classification signals.
- The critic cannot silently destroy good responses when unsure.
- Gemini provider works correctly with the multi-call-per-turn pipeline.

### Negative / Trade-offs

- Wider context windows increase token consumption per turn.
- The TSI and critic now process more text, increasing latency slightly.
- The memory orchestrator's dynamic window is less aggressive — it trades some token efficiency for reliability.

## Alternatives considered

1. **Reduce the pipeline to a single AI call.**
   Rejected for now because the TSI and critic still provide value when they have adequate context. May revisit if latency becomes a bottleneck.

2. **Use embeddings for context selection instead of wider windows.**
   Deferred to a future iteration. Wider windows solve the immediate problem without adding infrastructure complexity.

3. **Remove the critic entirely.**
   Rejected because the critic catches real reopening failures. The soft-mode approach preserves its value without the destructive rewrite risk.
