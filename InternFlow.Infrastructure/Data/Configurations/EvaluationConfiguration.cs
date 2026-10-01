using InternFlow.Domain.Evaluations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InternFlow.Infrastructure.Data.Configurations;

public class EvaluationConfiguration : IEntityTypeConfiguration<Evaluation>
{
    public void Configure(EntityTypeBuilder<Evaluation> builder)
    {
        builder.ToTable("Evaluations", "dbo");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("NEWID()").ValueGeneratedOnAdd();
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20).IsUnicode(false);
        builder.Property(e => e.OverallComment).HasMaxLength(4000);
        builder.Property(e => e.SubmittedAt).HasColumnType("datetime2");
        builder.Property(e => e.CreatedAt).HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(e => e.UpdatedAt).HasColumnType("datetime2");
        builder.HasIndex(e => new { e.EvaluationPeriodId, e.InternId, e.EvaluatorId }).IsUnique();
        builder.HasOne(e => e.EvaluationPeriod).WithMany(p => p.Evaluations).HasForeignKey(e => e.EvaluationPeriodId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.Intern).WithMany(u => u.EvaluationsReceived).HasForeignKey(e => e.InternId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Evaluator).WithMany(u => u.EvaluationsGiven).HasForeignKey(e => e.EvaluatorId).OnDelete(DeleteBehavior.Restrict);
    }
}
