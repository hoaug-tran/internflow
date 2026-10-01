using InternFlow.Domain.Entities;
using InternFlow.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InternFlow.Infrastructure.Data.Configurations;

public class InternProfileConfiguration : IEntityTypeConfiguration<InternProfile>
{
    public void Configure(EntityTypeBuilder<InternProfile> builder)
    {
        // 1. Tên bảng & Check Constraint
        builder.ToTable("InternProfiles", "dbo", t =>
        {
            t.HasCheckConstraint("CK_InternProfiles_Role", "[UserRole] = 'Intern'");
        });

        // 2. Khóa chính (1 - 1 với Users)
        builder.HasKey(ip => ip.UserId)
               .HasName("PK_InternProfiles");

        builder.Property(ip => ip.UserId)
               .ValueGeneratedNever();

        // 3. Cấu hình cột UserRole để tham gia khóa ngoại ghép
        builder.Property(ip => ip.UserRole)
               .HasConversion<string>()
               .HasMaxLength(20)
               .IsUnicode(false)
               .HasDefaultValue(UserRole.Intern)
               .IsRequired();

        // 4. Cấu hình các thuộc tính
        builder.Property(ip => ip.StudentCode)
               .HasMaxLength(30)
               .IsUnicode(false);

        // Filtered Unique Index: Mỗi sinh viên chỉ có 1 mã SV duy nhất (bỏ qua NULL)
        builder.HasIndex(ip => ip.StudentCode)
               .IsUnique()
               .HasFilter("[StudentCode] IS NOT NULL")
               .HasDatabaseName("UX_InternProfiles_StudentCode");

        builder.Property(ip => ip.University)
               .HasMaxLength(200)
               .IsUnicode(true);

        builder.Property(ip => ip.Major)
               .HasMaxLength(150)
               .IsUnicode(true);

        builder.Property(ip => ip.Phone)
               .HasMaxLength(20)
               .IsUnicode(false);

        builder.Property(ip => ip.DateOfBirth)
               .HasColumnType("date");

        builder.Property(ip => ip.CvUrl)
               .HasMaxLength(500)
               .IsUnicode(true);

        builder.Property(ip => ip.CreatedAt)
               .HasColumnType("datetime2")
               .HasDefaultValueSql("SYSUTCDATETIME()")
               .ValueGeneratedOnAdd();

        builder.Property(ip => ip.UpdatedAt)
               .HasColumnType("datetime2")
               .IsRequired(false);

        // 5. Khóa ngoại ghép (UserId, UserRole) -> Users(Id, Role)
        builder.HasOne(ip => ip.User)
               .WithOne(u => u.InternProfile)
               .HasPrincipalKey<User>(u => new { u.Id, u.Role })
               .HasForeignKey<InternProfile>(ip => new { ip.UserId, ip.UserRole })
               .HasConstraintName("FK_InternProfiles_Users")
               .OnDelete(DeleteBehavior.Cascade);
    }
}
