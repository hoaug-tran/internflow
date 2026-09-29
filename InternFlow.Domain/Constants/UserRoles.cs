namespace InternFlow.Domain.Constants;

public static class UserRoles
{
    public const string Admin = "Admin";
    public const string Mentor = "Mentor";
    public const string Intern = "Intern";

    public static readonly string[] All = [Admin, Mentor, Intern];
}