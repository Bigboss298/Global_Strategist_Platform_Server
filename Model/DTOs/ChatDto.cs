using Global_Strategist_Platform_Server.Model.Enum;

namespace Global_Strategist_Platform_Server.Model.DTOs;

// Request DTOs
public class SendMessageRequest
{
    public Guid ChatRoomId { get; set; }
    public string Content { get; set; } = string.Empty;
}

public class CreateDirectChatRequest
{
    public Guid OtherUserId { get; set; }
}

public class CreateProjectChatRequest
{
    public Guid ProjectId { get; set; }
}

// Response DTOs
public class ChatMessageResponse
{
    public Guid Id { get; set; }
    public Guid ChatRoomId { get; set; }
    public Guid SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string SenderProfilePhotoUrl { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
}

public class ChatParticipantResponse
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string ProfilePhotoUrl { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
}

public class ChatRoomResponse
{
    public Guid Id { get; set; }
    public RoomType RoomType { get; set; }
    public Guid? ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ChatParticipantResponse> Participants { get; set; } = new();
    public ChatMessageResponse? LastMessage { get; set; }
    public int UnreadCount { get; set; }
}

public class ChatMessagesPagedResult
{
    public List<ChatMessageResponse> Items { get; set; } = new();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPrevious => PageNumber > 1;
    public bool HasNext => PageNumber < TotalPages;
}
