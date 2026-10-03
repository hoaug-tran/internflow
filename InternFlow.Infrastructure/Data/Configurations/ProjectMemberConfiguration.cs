using InternFlow.Domain.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InternFlow.Infrastructure.Data.Configurations;

public class ProjectMemberConfiguration : IEntityTypeConfiguration<ProjectMember>
{
    public void Configure(EntityTypeBuilder<ProjectMember> builder)
    {
        builder.ToTable("ProjectMembers", "dbo");
        builder.HasKey(pm => new { pm.ProjectId, pm.UserId });
        builder.Property(pm => pm.Role).HasConversion<string>().HasMaxLength(20).IsUnicode(false);
        builder.Property(pm => pm.JoinedAt).HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(pm => pm.RemovedAt).HasColumnType("datetime2");
        builder.HasIndex(pm => new { pm.ProjectId, pm.UserId })
               .IsUnique()
               .HasFilter("[RemovedAt] IS NULL")
               .HasDatabaseName("UX_ProjectMembers_Project_User_Active");
        builder.HasOne(pm => pm.Project).WithMany(p => p.Members).HasForeignKey(pm => pm.ProjectId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(pm => pm.User).WithMany(u => u.ProjectMemberships).HasForeignKey(pm => pm.UserId).OnDelete(DeleteBehavior.Restrict);
    }
}
