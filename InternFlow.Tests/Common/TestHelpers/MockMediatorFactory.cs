using InternFlow.Application.Common.Caching;
using InternFlow.Application.Common.Interfaces;
using MediatR;
using Moq;

namespace InternFlow.Tests.Common.TestHelpers;

public static class MockMediatorFactory
{
    /// <summary>
    /// Tạo IMediator mock với Publish không làm gì.
    /// Dùng cho handler chỉ cần IMediator để publish notification
    /// (không cần test side-effect).
    /// </summary>
    public static Mock<IMediator> CreateNoOp()
    {
        var mock = new Mock<IMediator>();
        mock.Setup(m => m.Publish(
                It.IsAny<INotification>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return mock;
    }

    /// <summary>
    /// Tạo IMediator mock có capture notification để verify.
    /// Trả về mock + list notification đã publish.
    /// </summary>
    public static (Mock<IMediator> Mock, List<INotification> Published) CreateCapturing()
    {
        var published = new List<INotification>();
        var mock = new Mock<IMediator>();
        mock.Setup(m => m.Publish(
                It.IsAny<INotification>(),
                It.IsAny<CancellationToken>()))
            .Callback<INotification, CancellationToken>((n, _) => published.Add(n))
            .Returns(Task.CompletedTask);
        return (mock, published);
    }

}
