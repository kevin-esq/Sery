# 0010. Relevant Memory Retrieval and User Facts

Date: 2026-04-22

## Status

Accepted

## Context

The chat pipeline already persisted:

- conversation summaries,
- emotional memory,
- efficacy memory,
- user agent customization.

That was enough for broad continuity, but still weak for two common companion-AI behaviors:

1. remembering stable user facts across conversations,
2. pulling back only the most relevant prior context for the current turn.

Without those two layers, the system relied too heavily on:

- the latest conversation window,
- compressed summaries,
- model inference over broad memory text.

That made the system more likely to:

- miss stable facts the user had already shared,
- overfit to recent turns and forget older but relevant context,
- use generic continuity instead of precise continuity.

## Decision

We introduced:

1. persisted `UserMemoryFact`,
2. relevance-based retrieval for prior user snippets,
3. relevance-based retrieval for persisted facts,
4. heuristic fact extraction from user turns.

## Design

### `UserMemoryFact`

`UserMemoryFact` stores stable or semi-stable facts about the user:

- category,
- content,
- normalized key for deduplication,
- source conversation,
- timestamps.

Examples:

- identity facts,
- preference facts,
- response style preferences,
- stable personal context.

### Fact extraction

`IUserFactExtractor` is responsible for converting a user turn into candidate persisted facts.

Current implementation:

- `HeuristicUserFactExtractor`

The extractor currently favors precision over recall. It captures only a small set of interpretable patterns instead of trying to persist everything the user says.

### Relevant retrieval

The chat pipeline now retrieves:

- recent conversation history,
- relevant cross-conversation user snippets,
- relevant persisted facts.

Relevance is currently lexical and lightweight:

- normalized token overlap,
- phrase match boost,
- small recency boost.

This avoids adding embeddings or vector infrastructure in the current iteration while still improving practical continuity.

## Why this design

### Product fit

Companion-style systems need both:

- short-term continuity,
- long-term personal continuity.

Summaries alone are too coarse for that. Fact memory gives the system something more precise and durable.

### Clean architecture

- extraction is separate from persistence,
- ranking is separate from prompt composition,
- persistence remains inside infrastructure,
- orchestration remains inside `ChatMessageService`.

## Consequences

### Positive

- better recall of user identity and preferences,
- more precise continuity across sessions,
- less dependence on very long history windows,
- extensible path toward future semantic retrieval.

### Negative

- more moving parts in the memory pipeline,
- heuristic extraction can miss facts or over-normalize them,
- lexical relevance is useful but still weaker than embeddings.

## Follow-up work

1. Add fact review/edit APIs for the user.
2. Add fact expiration and confidence scoring.
3. Add semantic retrieval when telemetry justifies the added complexity.
4. Add richer fact categories such as goals, routines, and recurring stressors.
