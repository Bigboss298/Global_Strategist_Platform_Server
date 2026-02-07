using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Global_Strategist_Platform_Server.Model.Entities;

namespace Global_Strategist_Platform_Server.Data.EntityConfigurations;

public class ChatRoomConfiguration : IEntityTypeConfiguration<ChatRoom>
{
    public void Configure(EntityTypeBuilder<ChatRoom> builder)
    {
        builder.ToTable("ChatRooms");

        builder.HasKey(cr => cr.Id);

        builder.Property(cr => cr.RoomType)
            .IsRequired();

        builder.HasIndex(cr => cr.ProjectId);
        builder.HasIndex(cr => cr.RoomType);

        // Relationship with Project
        builder.HasOne(cr => cr.Project)
            .WithMany()
            .HasForeignKey(cr => cr.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
