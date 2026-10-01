using InternFlow.Domain.Entities;
using InternFlow.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InternFlow.Infrastructure.Data.Configurations;

public class MentorProfileConfiguration : IEntityTypeConfiguration<MentorProfile>
{
    public void Configure(EntityTypeBuilder<MentorProfile> builder)
    {
        // 1. Tên bảng & Check Constraint
        builder.ToTable("MentorProfiles", "dbo", t =>
        {
            t.HasCheckConstraint("CK_MentorProfiles_Role", "[UserRole] = 'Mentor'");
        });

        // 2. Khóa chính (1 - 1 với Users)
        builder.HasKey(mp => mp.UserId)
               .HasName("PK_MentorProfiles");

        builder.Property(mp => mp.UserId)
               .ValueGeneratedNever();

        // 3. Cấu hình cột UserRole để tham gia khóa ngoại ghép
        builder.Property(mp => mp.UserRole)
               .HasConversion<string>()
               .HasMaxLength(20)
               .IsUnicode(false)
               .HasDefaultValue(UserRole.Mentor)
               .IsRequired();

        // 4. Cấu hình các thuộc tính
        builder.Property(mp => mp.Department)
               .HasMaxLength(150)
               .IsUnicode(true);

        builder.Property(mp => mp.JobTitle)
               .HasMaxLength(150)
               .IsUnicode(true);

        builder.Property(mp => mp.Expertise)
               .HasMaxLength(300)
               .IsUnicode(true);

        builder.Property(mp => mp.Phone)
               .HasMaxLength(20)
               .IsUnicode(false);

        builder.Property(mp => mp.MaxMentees)
               .IsRequired(false);

        builder.Property(mp => mp.CreatedAt)
               .HasColumnType("datetime2")
               .HasDefaultValueSql("SYSUTCDATETIME()")
               .ValueGeneratedOnAdd();

        builder.Property(mp => mp.UpdatedAt)
               .HasColumnType("datetime2")
               .IsRequired(false);

        // 5. Khóa ngoại ghép (UserId, UserRole) -> Users(Id, Role)
        builder.HasOne(mp => mp.User)
               .WithOne(u => u.MentorProfile)
               .HasPrincipalKey<User>(u => new { u.Id, u.Role })
               .HasForeignKey<MentorProfile>(mp => new { mp.UserId, mp.UserRole })
               .HasConstraintName("FK_MentorProfiles_Users")
               .OnDelete(DeleteBehavior.Cascade);
    }
}
