using InternFlow.Infrastructure.Data.ReadModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InternFlow.Infrastructure.Data.Configurations;

public class CurrentMentorAssignmentViewConfiguration : IEntityTypeConfiguration<CurrentMentorAssignmentView>
{
    public void Configure(EntityTypeBuilder<CurrentMentorAssignmentView> builder)
    {
        // 1. Cấu hình Keyless Entity Type & gắn với Database View
        builder.HasNoKey();
        builder.ToView("vw_CurrentMentorAssignments", "dbo");

        // 2. Cấu hình các cột tương ứng
        builder.Property(v => v.AssignmentId)
               .HasColumnName("AssignmentId");

        builder.Property(v => v.BatchId)
               .HasColumnName("BatchId");

        builder.Property(v => v.InternId)
               .HasColumnName("InternId");

        builder.Property(v => v.MentorId)
               .HasColumnName("MentorId");

        builder.Property(v => v.AssignedAt)
               .HasColumnName("AssignedAt")
               .HasColumnType("datetime2");
    }
}
