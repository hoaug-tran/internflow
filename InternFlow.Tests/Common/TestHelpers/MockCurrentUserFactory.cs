using InternFlow.Application.Common.Interfaces;
using InternFlow.Domain.Identity;
using Moq;

namespace InternFlow.Tests.Common.TestHelpers;

public static class MockCurrentUserFactory
{
    public static ICurrentUserService Create(Guid userId, UserRole role)
    {
        var m = new Mock<ICurrentUserService>();
        m.Setup(x => x.UserId).Returns(userId);
        m.Setup(x => x.Email).Returns($"{role.ToString().ToLower()}@test.com");
        m.Setup(x => x.Role).Returns(role.ToString());
        m.Setup(x => x.IsAuthenticated).Returns(true);
        m.Setup(x => x.IsInRole(It.IsAny<string>()))
            .Returns<string>(r => string.Equals(
                r, role.ToString(), StringComparison.OrdinalIgnoreCase));
        return m.Object;
    }

    public static ICurrentUserService Anonymous()
    {
        var m = new Mock<ICurrentUserService>();
        m.Setup(x => x.UserId).Returns((Guid?)null);
        m.Setup(x => x.IsAuthenticated).Returns(false);
        m.Setup(x => x.Role).Returns((string?)null);
        m.Setup(x => x.IsInRole(It.IsAny<string>())).Returns(false);
        return m.Object;
    }
}
