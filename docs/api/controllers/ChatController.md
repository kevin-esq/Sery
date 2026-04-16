# ChatController

Path: `src/Sery.API/Controllers/V1/ChatController.cs`  
Namespace: `Sery.API.Controllers.V1`

## Endpoint

- `POST /api/chat/message`
- `POST /api/v1/chat/message`

## Outcomes

- `202 Accepted`
  - Message is accepted and queued for processing.
  - Response contract: `SendMessageAcceptedResponse`.
- `400 Bad Request`
  - Validation failure (`UserId` empty or `Message` blank).
  - Response contract: `ProblemDetails`.
  - Includes:
    - `code`: `SERY-API-400-002`
    - `messageKey`: `error.chat.required_userid_message`

## Example Request

```json
{
  "userId": "7d06b9f5-2fdb-44d8-88e8-9b2c49a23c5a",
  "message": "Hola Sery, hoy me siento nervioso."
}
```

## Example 202 Response

```json
{
  "userId": "7d06b9f5-2fdb-44d8-88e8-9b2c49a23c5a",
  "message": "Hola Sery, hoy me siento nervioso.",
  "status": "queued"
}
```

## Example 400 Response

```json
{
  "type": "https://httpstatuses.com/400",
  "title": "Validation failed.",
  "status": 400,
  "detail": "UserId and Message are required.",
  "instance": "/api/v1/chat/message",
  "code": "SERY-API-400-002",
  "messageKey": "error.chat.required_userid_message"
}
```
