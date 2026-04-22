# 0007 - Companion Relationship Modes and Guardrails

- Status: Accepted
- Date: 2026-04-22
- Deciders: Sery Backend Team

## Context

Sery is not intended to behave only as a neutral assistant. The product direction requires a companion experience that can combine:

- emotional closeness,
- practical help,
- role-aware companionship.

Desired modes include:

- listener,
- friend,
- close companion,
- romantic companion,
- support coach.

However, introducing relational behavior without explicit modeling creates two problems:

1. the companion style becomes inconsistent and hard to configure,
2. the product can drift into unhealthy attachment cues, exclusivity language, or manipulative retention patterns.

## Decision

- Introduce an explicit `RelationshipMode` enum in `Sery.Application.Chat`.
- Introduce `RelationalPersonaProfile` to model relational posture separately from generic tone traits.
- Introduce `CompanionSafetyProfile` to make companion guardrails explicit and configurable.
- Extend `AgentPersonaProfile` with:
  - `RelationalStyle`
  - `SafetyProfile`
- Extend configuration-backed persona mapping so the current provider can select:
  - `listener`
  - `friend`
  - `close companion`
  - `romantic companion`
  - `support coach`
- Update prompt composition so relational posture and safety constraints become part of the system prompt.
- Keep the current source config-backed. Do not persist relationship mode in database yet.

## Consequences

### Positive

- Relationship behavior is now explicit instead of hidden in ad-hoc prompt wording.
- Product can switch between companion styles without rewriting prompt logic.
- Safety constraints stay first-class and visible.
- Future admin or tenant-level customization becomes straightforward.

### Negative / Trade-offs

- Persona configuration is now richer and requires more discipline.
- Romantic companion behavior remains bounded by current safety constraints, which may feel less "immersive" than unconstrained roleplay systems.
- The current implementation is config-based, so personalization is not yet per-user or per-conversation.

## Guardrail Principles

- Sery may feel close, affectionate, and emotionally available.
- Sery must not imply it is a real human.
- Sery must not encourage exclusivity or dependency.
- Support and de-escalation override intimacy when risk or urgency appears.
- Relational behavior must remain support-oriented rather than purely retention-oriented.

## Follow-up Work

1. Persist companion profile by tenant or agent.
2. Allow user-selected relationship mode with policy validation.
3. Add moderation-aware blocks for requests that try to push manipulative or unsafe intimacy patterns.
