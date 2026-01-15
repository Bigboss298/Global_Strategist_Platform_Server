using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Global_Strategist_Platform_Server.Model.Entities;

namespace Global_Strategist_Platform_Server.Data.EntityConfigurations;

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable("Comments");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Content)
            .IsRequired()
            .HasMaxLength(2000);

        builder.HasIndex(c => c.ReportId);
        builder.HasIndex(c => c.StrategistId);

        // Relationships
        builder.HasOne(c => c.Report)
            .WithMany(r => r.Comments)
            .HasForeignKey(c => c.ReportId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Strategist)
            .WithMany()
            .HasForeignKey(c => c.StrategistId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

