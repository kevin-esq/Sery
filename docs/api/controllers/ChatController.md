# ChatController

Path: `src/Sery.API/Controllers/V1/ChatController.cs`  
Namespace: `Sery.API.Controllers.V1`

## Endpoint

- `POST /api/v1/chat/stream`

## Outcomes

- `200 OK`
  - Response is streamed as Server-Sent Events (`text/event-stream`).
- `400 Bad Request`
  - Validation failure (`Message` blank).
  - Response contract: `ProblemDetails`.
  - Includes:
    - `code`: `SERY-API-400-002`
    - `messageKey`: `error.chat.required_message`
- `401 Unauthorized`
  - Missing or invalid bearer token.

## Example Request

```json
{
  "message": "Hola Sery, hoy me siento nervioso.",
  "conversationId": "1c9a18d3-18b3-4f45-8bc3-bcb6b9fe7f38"
}
```

Header:

```http
Authorization: Bearer {accessToken}
```

If `conversationId` is omitted, the API creates a new conversation.

## Retention behavior

The internal chat pipeline now applies:

- turn analysis per user message,
- AI-backed turn-state interpretation with heuristic fallback,
- memory read/write orchestration,
- adaptive pacing per turn,
- configurable agent persona traits,
- conversation mode selection,
- response policy overrides,
- lightweight connection hooks,
- response critique before final emission,
- emotional memory,
- efficacy memory,
- compressed conversation memory.

This is designed to improve continuity, pacing, and engagement without moving conversational intelligence into the controller or transport layer.

### Internal pipeline summary

1. Load conversation context and cross-conversation memory.
2. Persist the user message.
3. Analyze the turn:
   - emotion,
   - urgency,
   - disclosure depth,
   - user need.
4. Build adaptive pacing:
   - response length,
   - tempo,
   - question budget,
   - structural density.
5. Interpret conversational state:
   - closure,
   - topic shift,
   - reopen risk.
6. Decide memory read/write policy for the turn.
7. Select the conversation mode and relational strategy.
8. Apply response policy overrides.
9. Generate connection hooks for the turn.
10. Compose the final system prompt.
11. Generate a draft answer, critique it, and then emit the final answer over SSE.
12. Refresh conversation summary, emotional memory, and efficacy memory after the assistant reply is stored.

### Notes for frontend consumers

- If `conversationId` is omitted, a new conversation is created automatically.
- The final SSE chunk always contains the `conversationId`, so the frontend can continue the same thread on the next turn.
- Adaptive behavior is internal. The request contract stays simple and stable even though the prompt generation is now more advanced.
- SSE remains the transport contract, but the current implementation emits validated final chunks, not raw provider tokens, because a response critic runs before emission.

## Example stream events

```text
data: {"t":"Hello ","f":false,"c":"11111111-1111-1111-1111-111111111111"}

data: {"t":"from integration stub","f":false,"c":"11111111-1111-1111-1111-111111111111"}

data: {"t":"","f":true,"c":"11111111-1111-1111-1111-111111111111"}
```

## Example 400 Response

```json
{
  "type": "https://httpstatuses.com/400",
  "title": "Validation failed.",
  "status": 400,
  "detail": "Message is required.",
  "instance": "/api/v1/chat/stream",
  "code": "SERY-API-400-002",
  "messageKey": "error.chat.required_message"
}
```
