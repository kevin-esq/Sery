# AgentCustomizationController

Path: `src/Sery.API/Controllers/V1/AgentCustomizationController.cs`  
Namespace: `Sery.API.Controllers.V1`

## Endpoints

- `GET /api/v1/agent-profile`
- `PATCH /api/v1/agent-profile`
- `DELETE /api/v1/agent-profile`

## Purpose

These endpoints let an authenticated user personalize how Sery behaves for them without changing the global application configuration.

The personalization is persisted in database and automatically applied during chat prompt composition.

## What users can customize

- identity presentation
- core demeanor
- warmth / directness / sincerity / charisma / playfulness
- reflection / proactivity / emotional expressiveness
- preferred response length
- follow-up question behavior
- action-step behavior
- relationship mode:
  - `listener`
  - `friend`
  - `close-companion`
  - `romantic-companion`
  - `support-coach`
- closeness / tenderness / protectiveness / flirtiness
- affectionate language
- romantic framing
- support-over-roleplay preference

## What users cannot customize

System safety guardrails remain controlled by the backend defaults:

- explicit AI transparency
- anti-exclusivity behavior
- anti-dependency rules
- crisis de-escalation
- sexual content restrictions

## GET /api/v1/agent-profile

Returns the effective profile for the current user.

If the user has no stored customization yet, the response still returns a valid profile resolved from system defaults.

### 200 Example

```json
{
  "agentName": "Sery",
  "identityPresentation": "neutral",
  "coreDemeanor": "supportive",
  "warmth": 78,
  "directness": 58,
  "sincerity": 88,
  "charisma": 62,
  "playfulness": 18,
  "reflection": 72,
  "proactivity": 66,
  "emotionalExpressiveness": 54,
  "preferredResponseLength": "medium",
  "askFollowUpQuestions": true,
  "offerActionSteps": true,
  "relationshipMode": "Friend",
  "closeness": 72,
  "tenderness": 68,
  "protectiveness": 56,
  "flirtiness": 0,
  "usesAffectionateLanguage": false,
  "allowsRomanticFraming": false,
  "prioritizeSupportOverRoleplay": true,
  "isCustomized": false,
  "updatedAt": null
}
```

## PATCH /api/v1/agent-profile

Applies a partial update to the user-specific customization.

### Validation rules

- numeric fields must be between `0` and `100`
- `identityPresentation` must be `neutral`, `feminine`, or `masculine`
- `preferredResponseLength` must be `short`, `medium`, `medium-long`, or `long`
- `relationshipMode` must be one of the supported values
- `coreDemeanor` cannot be blank

### Example request

```json
{
  "relationshipMode": "romantic-companion",
  "warmth": 85,
  "tenderness": 82,
  "usesAffectionateLanguage": true,
  "allowsRomanticFraming": true,
  "prioritizeSupportOverRoleplay": true
}
```

### 400 Example

```json
{
  "type": "https://httpstatuses.com/400",
  "title": "Validation failed.",
  "status": 400,
  "detail": "Numeric customization values must be between 0 and 100.",
  "instance": "/api/v1/agent-profile",
  "code": "SERY-API-400-005",
  "messageKey": "error.agent.invalid_customization"
}
```

## DELETE /api/v1/agent-profile

Removes the stored user customization and falls back to system defaults for future requests.
