# HealthController

Path: `src/Sery.API/Controllers/V1/HealthController.cs`  
Namespace: `Sery.API.Controllers.V1`

## Endpoint

- `GET /api/health`
- `GET /api/v1/health` (compatibility route)

## Outcomes

- `200 OK`
  - API service is alive and reachable.
  - Response contract: `HealthResponse`.

## Example 200 Response

```json
{
  "status": "ok",
  "service": "Sery.API"
}
```
