using InternFlow.Application.Common.Interfaces;
using Moq;

namespace InternFlow.Tests.Common.TestHelpers;

public static class MockInternshipQueryFactory
{
    public static Mock<IInternshipQueryService> Create(
        bool inBatch = true,
        bool mentorOfIntern = true)
    {
        var m = new Mock<IInternshipQueryService>();

        m.Setup(x => x.IsInternInBatchAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(inBatch);

        m.Setup(x => x.IsMentorAssignedToInternAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(mentorOfIntern);

        // Mặc định: intern active, workDate trong batch, mentor không có intern
        m.Setup(x => x.IsInternActiveInBatchAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        m.Setup(x => x.IsWorkDateInBatchAsync(
                It.IsAny<Guid>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        m.Setup(x => x.GetInternsOfMentorAsync(
                It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Guid>());

        return m;
    }

    /// <summary>
    /// Tạo mock với cấu hình chi tiết cho WorkLog test.
    /// </summary>
    public static Mock<IInternshipQueryService> CreateForWorkLog(
        bool internActive = true,
        bool workDateInBatch = true)
    {
        var m = new Mock<IInternshipQueryService>();

        m.Setup(x => x.IsInternActiveInBatchAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(internActive);

        m.Setup(x => x.IsWorkDateInBatchAsync(
                It.IsAny<Guid>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(workDateInBatch);

        return m;
    }

    /// <summary>
    /// Tạo mock cho Mentor xem worklog của intern.
    /// </summary>
    public static Mock<IInternshipQueryService> CreateForMentorView(
        params Guid[] internIds)
    {
        var m = new Mock<IInternshipQueryService>();
        m.Setup(x => x.GetInternsOfMentorAsync(
                It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(internIds.ToList());
        return m;
    }
}
