using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Global_Strategist_Platform_Server.Model.Entities;

namespace Global_Strategist_Platform_Server.Data.EntityConfigurations;

public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.ToTable("ChatMessages");

        builder.HasKey(cm => cm.Id);

        builder.Property(cm => cm.Content)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(cm => cm.IsRead)
            .HasDefaultValue(false);

        builder.HasIndex(cm => cm.ChatRoomId);
        builder.HasIndex(cm => cm.SenderId);
        builder.HasIndex(cm => cm.DateCreated);

        // Relationship with ChatRoom
        builder.HasOne(cm => cm.ChatRoom)
            .WithMany(cr => cr.Messages)
            .HasForeignKey(cm => cm.ChatRoomId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship with User (Sender)
        builder.HasOne(cm => cm.Sender)
            .WithMany()
            .HasForeignKey(cm => cm.SenderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
