using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Global_Strategist_Platform_Server.Model.Entities;

namespace Global_Strategist_Platform_Server.Data.EntityConfigurations;

public class ChatParticipantConfiguration : IEntityTypeConfiguration<ChatParticipant>
{
    public void Configure(EntityTypeBuilder<ChatParticipant> builder)
    {
        builder.ToTable("ChatParticipants");

        builder.HasKey(cp => cp.Id);

        builder.HasIndex(cp => cp.ChatRoomId);
        builder.HasIndex(cp => cp.UserId);
        builder.HasIndex(cp => new { cp.ChatRoomId, cp.UserId }).IsUnique();

        // Relationship with ChatRoom
        builder.HasOne(cp => cp.ChatRoom)
            .WithMany(cr => cr.Participants)
            .HasForeignKey(cp => cp.ChatRoomId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship with User
        builder.HasOne(cp => cp.User)
            .WithMany()
            .HasForeignKey(cp => cp.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
