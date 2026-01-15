using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Global_Strategist_Platform_Server.Model.Entities;
using System.Text.Json;

namespace Global_Strategist_Platform_Server.Data.EntityConfigurations;

public class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> builder)
    {
        builder.ToTable("Reports");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Title)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(r => r.Content)
            .IsRequired();

        // Configure JSON columns for Attachments and Links
        var jsonOptions = new JsonSerializerOptions();
        
        builder.Property(r => r.Attachments)
            .HasConversion(
                v => JsonSerializer.Serialize(v, jsonOptions),
                v => JsonSerializer.Deserialize<List<string>>(v, jsonOptions) ?? new List<string>())
            .HasColumnType("jsonb");

        builder.Property(r => r.Links)
            .HasConversion(
                v => JsonSerializer.Serialize(v, jsonOptions),
                v => JsonSerializer.Deserialize<List<string>>(v, jsonOptions) ?? new List<string>())
            .HasColumnType("jsonb");

        builder.HasIndex(r => r.StrategistId);
        builder.HasIndex(r => r.CategoryId);
        builder.HasIndex(r => r.ProjectId);
        builder.HasIndex(r => r.FieldId);

        // Relationships
        builder.HasOne(r => r.Category)
            .WithMany()
            .HasForeignKey(r => r.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Project)
            .WithMany()
            .HasForeignKey(r => r.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

