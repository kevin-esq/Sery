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
  "message": "Hola Sery, hoy me siento nervioso."
}
```

Header:

```http
Authorization: Bearer {accessToken}
```

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
