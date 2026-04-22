# 0008 - User-Customizable Agent Persona

- Status: Accepted
- Date: 2026-04-22
- Deciders: Sery Backend Team

## Context

Sery already supports a configurable base persona and relational modes, but those settings were previously global configuration values. That was not sufficient for the product direction because each user may want the agent to feel different:

- more warm or more direct,
- more like a listener or more like a friend,
- more close or more structured,
- more affectionate or more neutral.

The product requirement is that the user can personalize the agent "to their taste" without forcing a global personality for every user.

At the same time, safety constraints must remain system-controlled. Users may customize tone and relationship framing, but they must not be able to disable guardrails such as:

- explicit AI transparency,
- blocking exclusivity cues,
- crisis de-escalation,
- anti-dependency rules,
- sexual-content restrictions.

## Decision

- Introduce `UserAgentCustomization` as a persisted one-to-one entity associated with `User`.
- Keep global defaults in `ChatAIOptions`.
- Merge the default persona with the user-specific customization at runtime through `AgentPersonaCustomizationMerger`.
- Load the customization as part of chat context so every turn uses the effective persona automatically.
- Expose authenticated endpoints for:
  - `GET /api/v1/agent-profile`
  - `PATCH /api/v1/agent-profile`
  - `DELETE /api/v1/agent-profile`
- Allow the user to customize:
  - identity presentation,
  - demeanor,
  - tone sliders,
  - response style,
  - relationship mode,
  - relational closeness/tenderness/protectiveness/flirtiness,
  - affectionate and romantic framing preferences.
- Do not persist safety overrides from the user. Safety remains owned by the system persona.

## Consequences

### Positive

- Persona becomes user-specific instead of globally static.
- Frontend can build a real personalization screen without editing server config.
- Chat runtime behavior adapts automatically through the existing persona pipeline.
- Safety remains centrally enforced.

### Negative / Trade-offs

- Persona resolution is now a merge between defaults and persisted customization.
- More schema and API surface area must be maintained.
- Relationship personalization becomes a product-sensitive area that requires careful frontend UX and moderation policy.

## Alternatives Considered

1. **Keep all persona customization in configuration only**
   Rejected because users cannot personalize their own agent.

2. **Let users fully override safety settings**
   Rejected because it would allow unsafe relationship dynamics.

3. **Persist only one coarse field such as `RelationshipMode`**
   Rejected because the product needs richer control than a single enum.

## Follow-up Work

1. Add UI for persona customization.
2. Add tenant-level defaults that can differ from global defaults.
3. Add profile presets such as `listener`, `best friend`, `gentle companion`, `support coach`.
4. Add analytics for which customizations improve retention and usefulness without increasing unsafe attachment patterns.
