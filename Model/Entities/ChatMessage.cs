namespace Global_Strategist_Platform_Server.Model.Entities;

public class ChatMessage : BaseEntity
{
    public Guid ChatRoomId { get; set; }
    public Guid SenderId { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsRead { get; set; }

    // Navigation properties
    public ChatRoom ChatRoom { get; set; } = null!;
    public User Sender { get; set; } = null!;
}
