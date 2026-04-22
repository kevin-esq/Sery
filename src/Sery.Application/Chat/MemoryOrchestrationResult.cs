namespace Sery.Application.Chat;

public sealed record MemoryOrchestrationResult(
    MemoryReadPlan ReadPlan,
    MemoryWritePlan WritePlan);
