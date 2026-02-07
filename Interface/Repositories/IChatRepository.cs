using Global_Strategist_Platform_Server.Model.Entities;

namespace Global_Strategist_Platform_Server.Interface.Repositories;

public interface IChatRepository
{
    Task<ChatRoom?> GetByIdAsync(Guid id);
    Task<ChatRoom?> GetByIdWithParticipantsAsync(Guid id);
    Task<ChatRoom?> GetDirectChatRoomAsync(Guid userAId, Guid userBId);
    Task<ChatRoom?> GetProjectChatRoomAsync(Guid projectId);
    Task<IEnumerable<ChatRoom>> GetUserChatRoomsAsync(Guid userId);
    Task<ChatRoom> CreateAsync(ChatRoom chatRoom);
    Task AddParticipantAsync(ChatParticipant participant);
    Task<bool> IsUserParticipantAsync(Guid roomId, Guid userId);
    Task<IEnumerable<Guid>> GetParticipantUserIdsAsync(Guid roomId);
    Task SaveChangesAsync();
}
