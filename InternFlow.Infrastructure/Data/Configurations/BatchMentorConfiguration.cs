using InternFlow.Domain.Identity;
using InternFlow.Domain.Internships;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InternFlow.Infrastructure.Data.Configurations;

public class BatchMentorConfiguration : IEntityTypeConfiguration<BatchMentor>
{
    public void Configure(EntityTypeBuilder<BatchMentor> builder)
    {
        // 1. Tên bảng & Check Constraint
        builder.ToTable("BatchMentors", "dbo", t =>
        {
            t.HasCheckConstraint("CK_BM_Role", "[MentorRole] = 'Mentor'");
        });

        // 2. Khóa chính ghép (Composite Primary Key)
        builder.HasKey(bm => new { bm.InternshipId, bm.MentorId })
               .HasName("PK_BatchMentors");

        builder.Property(bm => bm.InternshipId).HasColumnName("BatchId");

        // 3. Cấu hình cột MentorRole
        builder.Property(bm => bm.MentorRole)
               .HasConversion<string>()
               .HasMaxLength(20)
               .IsUnicode(false)
               .HasDefaultValue(UserRole.Mentor)
               .IsRequired();

        builder.Property(bm => bm.JoinedAt)
               .HasColumnType("datetime2")
               .HasDefaultValueSql("SYSUTCDATETIME()")
               .ValueGeneratedOnAdd();

        // 4. Chỉ mục tra cứu Mentor
        builder.HasIndex(bm => bm.MentorId)
               .HasDatabaseName("IX_BM_Mentor");

        // 5. Khóa ngoại
        // Tham chiếu Batch
        builder.HasOne(bm => bm.Internship)
               .WithMany(b => b.BatchMentors)
               .HasForeignKey(bm => bm.InternshipId)
               .HasConstraintName("FK_BM_Batch")
               .OnDelete(DeleteBehavior.Cascade);

        // Khóa ngoại ghép (MentorId, MentorRole) -> Users(Id, Role)
        builder.HasOne(bm => bm.Mentor)
               .WithMany(u => u.BatchMentors)
               .HasPrincipalKey(u => new { u.Id, u.Role })
               .HasForeignKey(bm => new { bm.MentorId, bm.MentorRole })
               .HasConstraintName("FK_BM_Mentor")
               .OnDelete(DeleteBehavior.Restrict);

        // Tham chiếu User thêm vào đợt
        builder.HasOne(bm => bm.AddedByUser)
               .WithMany(u => u.AddedBatchMentors)
               .HasForeignKey(bm => bm.AddedBy)
               .HasConstraintName("FK_BM_AddedBy")
               .OnDelete(DeleteBehavior.Restrict);
    }
}
