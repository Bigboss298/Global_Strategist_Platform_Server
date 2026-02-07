using Global_Strategist_Platform_Server.Model.Enum;

namespace Global_Strategist_Platform_Server.Model.Entities;

public class ChatRoom : BaseEntity
{
    public RoomType RoomType { get; set; }
    public Guid? ProjectId { get; set; }

    // Navigation properties
    public Project? Project { get; set; }
    public ICollection<ChatParticipant> Participants { get; set; } = new List<ChatParticipant>();
    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}
