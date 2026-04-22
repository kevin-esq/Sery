# ConversationsController

Path: `src/Sery.API/Controllers/V1/ConversationsController.cs`  
Namespace: `Sery.API.Controllers.V1`

## Endpoints

- `GET /api/v1/conversations`
- `GET /api/v1/conversations/{id}`
- `GET /api/v1/conversations/{id}/messages`
- `PATCH /api/v1/conversations/{id}`
- `DELETE /api/v1/conversations/{id}`

## Purpose

Provides authenticated conversation management:

- list conversation summaries
- inspect one conversation
- page through messages
- rename a conversation
- archive/unarchive
- pin/unpin
- delete

## Important metadata

Conversation summaries now include:

- `title`
- `summary`
- `createdAt`
- `updatedAt`
- `lastMessageAt`
- `messageCount`
- `isArchived`
- `isPinned`

These fields support continuity, retention-oriented UX, and future chat surfaces such as recent chats, pinned chats, and resumed sessions.

## Example `PATCH` request

```json
{
  "title": "Plan para bajar mi ansiedad esta semana",
  "isArchived": false,
  "isPinned": true
}
```

## Error outcomes

- `400 Bad Request`
  - no fields provided
  - invalid title
- `401 Unauthorized`
  - missing or invalid bearer token
- `404 Not Found`
  - conversation does not belong to the authenticated user or does not exist
