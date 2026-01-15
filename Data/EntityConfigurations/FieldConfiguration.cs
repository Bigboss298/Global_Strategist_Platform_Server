using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Global_Strategist_Platform_Server.Model.Entities;

namespace Global_Strategist_Platform_Server.Data.EntityConfigurations;

public class FieldConfiguration : IEntityTypeConfiguration<Field>
{
    public void Configure(EntityTypeBuilder<Field> builder)
    {
        builder.ToTable("Fields");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(f => f.ProjectId);

        // Relationships
        builder.HasMany(f => f.Reports)
            .WithOne(r => r.Field)
            .HasForeignKey(r => r.FieldId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

