namespace Global_Strategist_Platform_Server.Model.Entities;

public class ChatParticipant : BaseEntity
{
    public Guid ChatRoomId { get; set; }
    public Guid UserId { get; set; }

    // Navigation properties
    public ChatRoom ChatRoom { get; set; } = null!;
    public User User { get; set; } = null!;
}
