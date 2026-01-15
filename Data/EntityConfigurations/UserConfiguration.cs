using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Global_Strategist_Platform_Server.Model.Entities;
using Global_Strategist_Platform_Server.Model.Enum;

namespace Global_Strategist_Platform_Server.Data.EntityConfigurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(u => u.Headline)
            .HasMaxLength(300);

        builder.Property(u => u.Country)
            .HasMaxLength(100);

        builder.Property(u => u.ProfilePhotoUrl)
            .HasMaxLength(500);

        builder.Property(u => u.IsActive)
            .HasDefaultValue(true);

        builder.Property(u => u.Role)
            .IsRequired()
            .HasConversion<int>();

        // Relationships
        builder.HasMany(u => u.Reports)
            .WithOne(r => r.Strategist)
            .HasForeignKey(r => r.StrategistId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

