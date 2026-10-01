using InternFlow.Domain.Entities;
using InternFlow.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InternFlow.Infrastructure.Data.Configurations;

public class InternshipConfiguration : IEntityTypeConfiguration<Internship>
{
    public void Configure(EntityTypeBuilder<Internship> builder)
    {
        // 1. Tên bảng & Check Constraints
        builder.ToTable("Internships", "dbo", t =>
        {
            t.HasCheckConstraint("CK_Batches_Dates", "[EndDate] > [StartDate]");
            t.HasCheckConstraint("CK_Batches_Status", "[Status] IN ('Planned', 'Active', 'Closed')");
        });

        // 2. Khóa chính (Primary Key - UNIQUEIDENTIFIER / Guid)
        builder.HasKey(i => i.Id)
               .HasName("PK_Internships");

        builder.Property(i => i.Id)
               .HasDefaultValueSql("NEWID()")
               .ValueGeneratedOnAdd();

        // 3. Cấu hình các thuộc tính
        builder.Property(i => i.Name)
               .HasMaxLength(150)
               .IsUnicode(true)
               .IsRequired();

        builder.HasIndex(i => i.Name)
               .IsUnique()
               .HasDatabaseName("UQ_Batches_Name");

        builder.Property(i => i.StartDate)
               .HasColumnType("date")
               .IsRequired();

        builder.Property(i => i.EndDate)
               .HasColumnType("date")
               .IsRequired();

        builder.Property(i => i.Status)
               .HasConversion<string>()
               .HasMaxLength(20)
               .IsUnicode(false)
               .HasDefaultValue(BatchStatus.Planned)
               .IsRequired();

        builder.Property(i => i.CreatedAt)
               .HasColumnType("datetime2")
               .HasDefaultValueSql("SYSUTCDATETIME()")
               .ValueGeneratedOnAdd();

        builder.Property(i => i.UpdatedAt)
               .HasColumnType("datetime2")
               .IsRequired(false);

        // 4. Chỉ mục tìm kiếm trạng thái đợt
        builder.HasIndex(i => new { i.Status, i.StartDate })
               .HasDatabaseName("IX_Batches_Status");

        // 5. Khóa ngoại tới User tạo đợt
        builder.HasOne(i => i.Creator)
               .WithMany(u => u.CreatedInternships)
               .HasForeignKey(i => i.CreatedBy)
               .HasConstraintName("FK_Batches_CreatedBy")
               .OnDelete(DeleteBehavior.Restrict);
    }
}
