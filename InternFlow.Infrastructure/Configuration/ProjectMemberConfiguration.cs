using InternFlow.Domain.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InternFlow.Infrastructure.Persistence.Configurations;

public class ProjectMemberConfiguration : IEntityTypeConfiguration<ProjectMember>
{
    public void Configure(EntityTypeBuilder<ProjectMember> builder)
    {
        builder.ToTable("ProjectMembers", "dbo", tb =>
        {
            tb.HasCheckConstraint("CK_ProjectMembers_MemberRole", "[MemberRole] IN ('Owner','Member')");
            tb.HasCheckConstraint("CK_ProjectMembers_Removed", "[RemovedAt] IS NULL OR [RemovedAt] >= [JoinedAt]");
        });

        builder.HasKey(pm => pm.Id);

        builder.Property(pm => pm.MemberRole)
            .HasMaxLength(20)
            .HasDefaultValue("Member")
            .IsRequired();

        builder.Property(pm => pm.JoinedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        // Khóa ngoại nội bộ với Project
        builder.HasOne(pm => pm.Project)
            .WithMany(p => p.Members)
            .HasForeignKey(pm => pm.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        // Filtered Index: Chặn trùng thành viên đang active (RemovedAt IS NULL)
        builder.HasIndex(pm => new { pm.ProjectId, pm.UserId })
            .IsUnique()
            .HasFilter("[RemovedAt] IS NULL")
            .HasDatabaseName("UX_ProjectMembers_Project_User_Active");

        builder.HasIndex(pm => pm.UserId)
            .HasFilter("[RemovedAt] IS NULL")
            .HasDatabaseName("IX_ProjectMembers_User_Active");
    }
}