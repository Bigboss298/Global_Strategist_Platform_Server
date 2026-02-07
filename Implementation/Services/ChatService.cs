using Global_Strategist_Platform_Server.Interface.Repositories;
using Global_Strategist_Platform_Server.Interface.Services;
using Global_Strategist_Platform_Server.Model.DTOs;
using Global_Strategist_Platform_Server.Model.Entities;
using Global_Strategist_Platform_Server.Model.Enum;

namespace Global_Strategist_Platform_Server.Implementation.Services;

public class ChatService : IChatService
{
    private readonly IChatRepository _chatRepository;
    private readonly IChatMessageRepository _chatMessageRepository;
    private readonly IBaseRepository<User> _userRepository;
    private readonly IBaseRepository<Project> _projectRepository;

    public ChatService(
        IChatRepository chatRepository,
        IChatMessageRepository chatMessageRepository,
        IBaseRepository<User> userRepository,
        IBaseRepository<Project> projectRepository)
    {
        _chatRepository = chatRepository;
        _chatMessageRepository = chatMessageRepository;
        _userRepository = userRepository;
        _projectRepository = projectRepository;
    }

    public async Task<ChatRoomResponse> CreateOrGetDirectChatAsync(Guid userAId, Guid userBId)
    {
        if (userAId == userBId)
            throw new ArgumentException("Cannot create a chat with yourself.");

        // Validate both users exist
        var userA = await _userRepository.GetByIdAsync(userAId);
        var userB = await _userRepository.GetByIdAsync(userBId);

        if (userA == null || userB == null)
            throw new KeyNotFoundException("One or both users not found.");

        // Check if direct chat already exists
        var existingRoom = await _chatRepository.GetDirectChatRoomAsync(userAId, userBId);
        if (existingRoom != null)
        {
            return await MapToChatRoomResponseAsync(existingRoom, userAId);
        }

        // Create new direct chat room
        var chatRoom = new ChatRoom
        {
            Id = Guid.NewGuid(),
            RoomType = RoomType.Direct,
            ProjectId = null
        };

        await _chatRepository.CreateAsync(chatRoom);

        // Add participants
        await _chatRepository.AddParticipantAsync(new ChatParticipant
        {
            Id = Guid.NewGuid(),
            ChatRoomId = chatRoom.Id,
            UserId = userAId
        });

        await _chatRepository.AddParticipantAsync(new ChatParticipant
        {
            Id = Guid.NewGuid(),
            ChatRoomId = chatRoom.Id,
            UserId = userBId
        });

        await _chatRepository.SaveChangesAsync();

        // Fetch the created room with participants
        var createdRoom = await _chatRepository.GetByIdWithParticipantsAsync(chatRoom.Id);
        return await MapToChatRoomResponseAsync(createdRoom!, userAId);
    }

    public async Task<ChatRoomResponse> CreateOrGetProjectChatAsync(Guid projectId, Guid userId)
    {
        // Validate project exists
        var project = await _projectRepository.GetByIdAsync(projectId);
        if (project == null)
            throw new KeyNotFoundException($"Project with ID {projectId} not found.");

        // Validate user exists
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException($"User with ID {userId} not found.");

        // Check if project chat already exists
        var existingRoom = await _chatRepository.GetProjectChatRoomAsync(projectId);
        if (existingRoom != null)
        {
            // Add user as participant if not already
            var isParticipant = await _chatRepository.IsUserParticipantAsync(existingRoom.Id, userId);
            if (!isParticipant)
            {
                await _chatRepository.AddParticipantAsync(new ChatParticipant
                {
                    Id = Guid.NewGuid(),
                    ChatRoomId = existingRoom.Id,
                    UserId = userId
                });
                await _chatRepository.SaveChangesAsync();

                // Refetch with updated participants
                existingRoom = await _chatRepository.GetByIdWithParticipantsAsync(existingRoom.Id);
            }

            return await MapToChatRoomResponseAsync(existingRoom!, userId);
        }

        // Create new project chat room
        var chatRoom = new ChatRoom
        {
            Id = Guid.NewGuid(),
            RoomType = RoomType.Project,
            ProjectId = projectId
        };

        await _chatRepository.CreateAsync(chatRoom);

        // Add the creating user as first participant
        await _chatRepository.AddParticipantAsync(new ChatParticipant
        {
            Id = Guid.NewGuid(),
            ChatRoomId = chatRoom.Id,
            UserId = userId
        });

        await _chatRepository.SaveChangesAsync();

        // Fetch the created room with participants
        var createdRoom = await _chatRepository.GetByIdWithParticipantsAsync(chatRoom.Id);
        return await MapToChatRoomResponseAsync(createdRoom!, userId);
    }

    public async Task<ChatMessageResponse> SendMessageAsync(Guid roomId, Guid senderId, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Message content cannot be empty.");

        // Validate room exists
        var room = await _chatRepository.GetByIdAsync(roomId);
        if (room == null)
            throw new KeyNotFoundException($"Chat room with ID {roomId} not found.");

        // Validate sender is a participant
        var isParticipant = await _chatRepository.IsUserParticipantAsync(roomId, senderId);
        if (!isParticipant)
            throw new UnauthorizedAccessException("User is not a participant of this chat room.");

        // Validate sender exists
        var sender = await _userRepository.GetByIdAsync(senderId);
        if (sender == null)
            throw new KeyNotFoundException($"User with ID {senderId} not found.");

        // Create and persist message
        var message = new ChatMessage
        {
            Id = Guid.NewGuid(),
            ChatRoomId = roomId,
            SenderId = senderId,
            Content = content.Trim(),
            IsRead = false
        };

        await _chatMessageRepository.CreateAsync(message);
        await _chatMessageRepository.SaveChangesAsync();

        return new ChatMessageResponse
        {
            Id = message.Id,
            ChatRoomId = message.ChatRoomId,
            SenderId = message.SenderId,
            SenderName = sender.FullName,
            SenderProfilePhotoUrl = sender.ProfilePhotoUrl,
            Content = message.Content,
            CreatedAt = message.DateCreated,
            IsRead = message.IsRead
        };
    }

    public async Task<ChatMessagesPagedResult> GetMessagesAsync(Guid roomId, Guid userId, int page, int pageSize)
    {
        // Validate room exists
        var room = await _chatRepository.GetByIdAsync(roomId);
        if (room == null)
            throw new KeyNotFoundException($"Chat room with ID {roomId} not found.");

        // Validate user is a participant
        var isParticipant = await _chatRepository.IsUserParticipantAsync(roomId, userId);
        if (!isParticipant)
            throw new UnauthorizedAccessException("User is not a participant of this chat room.");

        var messages = await _chatMessageRepository.GetByRoomIdAsync(roomId, page, pageSize);
        var totalCount = await _chatMessageRepository.GetMessageCountAsync(roomId);

        return new ChatMessagesPagedResult
        {
            Items = [.. messages.Select(m => new ChatMessageResponse
            {
                Id = m.Id,
                ChatRoomId = m.ChatRoomId,
                SenderId = m.SenderId,
                SenderName = m.Sender.FullName,
                SenderProfilePhotoUrl = m.Sender.ProfilePhotoUrl,
                Content = m.Content,
                CreatedAt = m.DateCreated,
                IsRead = m.IsRead
            })],
            PageNumber = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<IEnumerable<ChatRoomResponse>> GetUserChatRoomsAsync(Guid userId)
    {
        // Validate user exists
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException($"User with ID {userId} not found.");

        var rooms = await _chatRepository.GetUserChatRoomsAsync(userId);
        var responses = new List<ChatRoomResponse>();

        foreach (var room in rooms)
        {
            responses.Add(await MapToChatRoomResponseAsync(room, userId));
        }

        // Order by last message date
        return responses.OrderByDescending(r => r.LastMessage?.CreatedAt ?? r.CreatedAt);
    }

    public async Task<bool> IsUserParticipantAsync(Guid roomId, Guid userId)
    {
        return await _chatRepository.IsUserParticipantAsync(roomId, userId);
    }

    public async Task MarkMessagesAsReadAsync(Guid roomId, Guid userId)
    {
        // Validate user is a participant
        var isParticipant = await _chatRepository.IsUserParticipantAsync(roomId, userId);
        if (!isParticipant)
            throw new UnauthorizedAccessException("User is not a participant of this chat room.");

        await _chatMessageRepository.MarkMessagesAsReadAsync(roomId, userId);
        await _chatMessageRepository.SaveChangesAsync();
    }

    public async Task<IEnumerable<Guid>> GetRoomParticipantsAsync(Guid roomId)
    {
        return await _chatRepository.GetParticipantUserIdsAsync(roomId);
    }

    private async Task<ChatRoomResponse> MapToChatRoomResponseAsync(ChatRoom room, Guid currentUserId)
    {
        var lastMessage = await _chatMessageRepository.GetLastMessageAsync(room.Id);
        var unreadCount = await _chatMessageRepository.GetUnreadCountAsync(room.Id, currentUserId);

        return new ChatRoomResponse
        {
            Id = room.Id,
            RoomType = room.RoomType,
            ProjectId = room.ProjectId,
            ProjectName = room.Project?.Name,
            CreatedAt = room.DateCreated,
            Participants = room.Participants.Select(p => new ChatParticipantResponse
            {
                UserId = p.UserId,
                FullName = p.User.FullName,
                ProfilePhotoUrl = p.User.ProfilePhotoUrl,
                JoinedAt = p.DateCreated
            }).ToList(),
            LastMessage = lastMessage != null ? new ChatMessageResponse
            {
                Id = lastMessage.Id,
                ChatRoomId = lastMessage.ChatRoomId,
                SenderId = lastMessage.SenderId,
                SenderName = lastMessage.Sender.FullName,
                SenderProfilePhotoUrl = lastMessage.Sender.ProfilePhotoUrl,
                Content = lastMessage.Content,
                CreatedAt = lastMessage.DateCreated,
                IsRead = lastMessage.IsRead
            } : null,
            UnreadCount = unreadCount
        };
    }
}
