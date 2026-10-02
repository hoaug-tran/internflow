using InternFlow.Domain.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InternFlow.Infrastructure.Data.Configurations;

public class WorkLogConfiguration : IEntityTypeConfiguration<WorkLog>
{
    public void Configure(EntityTypeBuilder<WorkLog> builder)
    {
        builder.ToTable("WorkLogs", "dbo", t => t.HasCheckConstraint("CK_WorkLogs_Hours", "[Hours] > 0 AND [Hours] <= 24"));
        builder.HasKey(w => w.Id);
        builder.Property(w => w.Id).HasDefaultValueSql("NEWID()").ValueGeneratedOnAdd();
        builder.Property(w => w.WorkDate).HasColumnType("date");
        builder.Property(w => w.Hours).HasPrecision(5, 2);
        builder.Property(w => w.Description).HasMaxLength(2000);
        builder.Property(w => w.CreatedAt).HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(w => w.UpdatedAt).HasColumnType("datetime2");
        builder.HasIndex(w => new { w.TaskItemId, w.WorkDate });
        builder.HasOne(w => w.TaskItem).WithMany(t => t.WorkLogs).HasForeignKey(w => w.TaskItemId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(w => w.User).WithMany(u => u.WorkLogs).HasForeignKey(w => w.UserId).OnDelete(DeleteBehavior.Restrict);
    }
}
