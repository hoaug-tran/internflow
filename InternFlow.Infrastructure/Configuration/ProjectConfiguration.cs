using InternFlow.Domain.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InternFlow.Infrastructure.Persistence.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects", "dbo", tb =>
    tb.HasCheckConstraint("CK_Projects_Status", "[Status] IN ('Active','Closed')"));

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasMaxLength(2000);

        builder.Property(p => p.Status)
            .HasMaxLength(20)
            .HasDefaultValue("Active")
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.Property(p => p.UpdatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.Property(p => p.RowVersion)
            .IsRowVersion();


        builder.HasIndex(p => new { p.InternshipId, p.Name })
            .IsUnique()
            .HasDatabaseName("UQ_Projects_Internship_Name");

        builder.HasIndex(p => new { p.InternshipId, p.Status })
            .HasDatabaseName("IX_Projects_Internship_Status");

        builder.HasIndex(p => p.CreatedBy)
            .HasDatabaseName("IX_Projects_CreatedBy");
    }
}
