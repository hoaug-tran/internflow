using InternFlow.Domain.Internships;

namespace InternFlow.Application.Features.Internships.DTOs;

public record CreateInternshipRequest(
    string Name,
    DateOnly StartDate,
    DateOnly EndDate
);

public record UpdateInternshipStatusRequest(
    InternshipStatus NewStatus
);

public record AddBatchMentorRequest(
    Guid MentorId
);

public record AddBatchInternRequest(
    Guid InternId,
    string? Notes = null
);

public record UpdateBatchInternStatusRequest(
    BatchInternStatus NewStatus
);

public record AssignMentorRequest(
    Guid InternId,
    Guid MentorId
);

public record EndMentorAssignmentRequest(
    string? EndReason = null
);
