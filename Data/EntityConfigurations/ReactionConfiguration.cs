using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Global_Strategist_Platform_Server.Model.Entities;

namespace Global_Strategist_Platform_Server.Data.EntityConfigurations;

public class ReactionConfiguration : IEntityTypeConfiguration<Reaction>
{
    public void Configure(EntityTypeBuilder<Reaction> builder)
    {
        builder.ToTable("Reactions");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.ReactionType)
            .IsRequired()
            .HasConversion<int>();

        builder.HasIndex(r => r.ReportId);
        builder.HasIndex(r => r.StrategistId);

        // Unique constraint: one reaction per user per report
        builder.HasIndex(r => new { r.ReportId, r.StrategistId })
            .IsUnique();

        // Relationships
        builder.HasOne(r => r.Report)
            .WithMany(report => report.Reactions)
            .HasForeignKey(r => r.ReportId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Strategist)
            .WithMany()
            .HasForeignKey(r => r.StrategistId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

