namespace InternFlow.Domain.Common;

public static class RevocationReasons
{
    public const string Logout = "Logout";
    public const string AdminRevoke = "AdminRevoke";
    public const string ReuseDetected = "ReuseDetected";
    public const string UserDeactivated = "UserDeactivated";
}
