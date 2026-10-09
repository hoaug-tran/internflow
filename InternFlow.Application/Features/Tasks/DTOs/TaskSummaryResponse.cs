namespace InternFlow.Application.Features.Tasks.DTOs;

public sealed record TaskSummaryResponse(
    Guid ProjectId,
    int TotalTasks,
    int TodoCount,
    int InProgressCount,
    int InReviewCount,
    int DoneCount,
    int CancelledCount,
    int OverdueCount,
    IReadOnlyList<TaskSummaryByAssignee> ByAssignee,
    DateTime GeneratedAtUtc);

public sealed record TaskSummaryByAssignee(
    Guid UserId,
    string FullName,
    int Total,
    int Done,
    int InProgress,
    int Overdue);
