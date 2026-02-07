using Global_Strategist_Platform_Server.Model.DTOs;
using Global_Strategist_Platform_Server.Model.Entities;

namespace Global_Strategist_Platform_Server.Interface.Services;

public interface IChatService
{
    Task<ChatRoomResponse> CreateOrGetDirectChatAsync(Guid userAId, Guid userBId);
    Task<ChatRoomResponse> CreateOrGetProjectChatAsync(Guid projectId, Guid userId);
    Task<ChatMessageResponse> SendMessageAsync(Guid roomId, Guid senderId, string content);
    Task<ChatMessagesPagedResult> GetMessagesAsync(Guid roomId, Guid userId, int page, int pageSize);
    Task<IEnumerable<ChatRoomResponse>> GetUserChatRoomsAsync(Guid userId);
    Task<bool> IsUserParticipantAsync(Guid roomId, Guid userId);
    Task MarkMessagesAsReadAsync(Guid roomId, Guid userId);
    Task<IEnumerable<Guid>> GetRoomParticipantsAsync(Guid roomId);
}
