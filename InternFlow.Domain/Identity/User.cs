using InternFlow.Domain.Common;
using InternFlow.Domain.Evaluations;
using InternFlow.Domain.Internships;
using InternFlow.Domain.Projects;

namespace InternFlow.Domain.Identity;

public class User : AuditableEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
    public int FailedLoginCount { get; set; }
    public DateTime? LockoutEnd { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public byte[] RowVersion { get; set; } = null!;

    public InternProfile? InternProfile { get; set; }
    public MentorProfile? MentorProfile { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<Internship> CreatedInternships { get; set; } = new List<Internship>();
    public ICollection<BatchMentor> BatchMentors { get; set; } = new List<BatchMentor>();
    public ICollection<BatchIntern> BatchInterns { get; set; } = new List<BatchIntern>();
    public ICollection<BatchMentor> AddedBatchMentors { get; set; } = new List<BatchMentor>();
    public ICollection<BatchIntern> AddedBatchInterns { get; set; } = new List<BatchIntern>();
    public ICollection<MentorAssignment> AssignedMentorAssignments { get; set; } = new List<MentorAssignment>();
    public ICollection<Project> CreatedProjects { get; set; } = new List<Project>();
    public ICollection<ProjectMember> ProjectMemberships { get; set; } = new List<ProjectMember>();
    public ICollection<TaskItem> AssignedTasks { get; set; } = new List<TaskItem>();
    public ICollection<WorkLog> WorkLogs { get; set; } = new List<WorkLog>();
    public ICollection<Evaluation> EvaluationsGiven { get; set; } = new List<Evaluation>();
    public ICollection<Evaluation> EvaluationsReceived { get; set; } = new List<Evaluation>();
}
