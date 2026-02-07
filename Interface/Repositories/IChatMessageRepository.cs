using Global_Strategist_Platform_Server.Model.Entities;

namespace Global_Strategist_Platform_Server.Interface.Repositories;

public interface IChatMessageRepository
{
    Task<ChatMessage?> GetByIdAsync(Guid id);
    Task<IEnumerable<ChatMessage>> GetByRoomIdAsync(Guid roomId, int page, int pageSize);
    Task<int> GetMessageCountAsync(Guid roomId);
    Task<ChatMessage?> GetLastMessageAsync(Guid roomId);
    Task<int> GetUnreadCountAsync(Guid roomId, Guid userId);
    Task<ChatMessage> CreateAsync(ChatMessage message);
    Task MarkMessagesAsReadAsync(Guid roomId, Guid userId);
    Task SaveChangesAsync();
}
