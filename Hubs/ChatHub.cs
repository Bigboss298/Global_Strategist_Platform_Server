using System.Collections.Concurrent;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Global_Strategist_Platform_Server.Interface.Services;
using Global_Strategist_Platform_Server.Model.DTOs;

namespace Global_Strategist_Platform_Server.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IChatService _chatService;
    private readonly ILogger<ChatHub> _logger;

    // Thread-safe mapping of UserId -> ConnectionIds (a user can have multiple connections)
    private static readonly ConcurrentDictionary<Guid, HashSet<string>> _userConnections = new();

    public ChatHub(IChatService chatService, ILogger<ChatHub> logger)
    {
        _chatService = chatService;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        if (userId.HasValue)
        {
            // Track user connection
            _userConnections.AddOrUpdate(
                userId.Value,
                new HashSet<string> { Context.ConnectionId },
                (key, existingSet) =>
                {
                    lock (existingSet)
                    {
                        existingSet.Add(Context.ConnectionId);
                    }
                    return existingSet;
                });

            // Auto-join user to all their chat room groups
            try
            {
                var userRooms = await _chatService.GetUserChatRoomsAsync(userId.Value);
                foreach (var room in userRooms)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, room.Id.ToString());
                    _logger.LogDebug("Auto-joined user {UserId} to room {RoomId}", userId, room.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to auto-join user {UserId} to their rooms", userId);
            }

            _logger.LogInformation("User {UserId} connected with connection {ConnectionId}", userId, Context.ConnectionId);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        if (userId.HasValue && _userConnections.TryGetValue(userId.Value, out var connections))
        {
            lock (connections)
            {
                connections.Remove(Context.ConnectionId);
                if (connections.Count == 0)
                {
                    _userConnections.TryRemove(userId.Value, out _);
                }
            }

            _logger.LogInformation("User {UserId} disconnected from connection {ConnectionId}", userId, Context.ConnectionId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinRoom(Guid roomId)
    {
        var userId = GetUserId();
        if (!userId.HasValue)
        {
            await Clients.Caller.SendAsync("Error", "User not authenticated.");
            return;
        }

        // Validate user is a participant
        var isParticipant = await _chatService.IsUserParticipantAsync(roomId, userId.Value);
        if (!isParticipant)
        {
            await Clients.Caller.SendAsync("Error", "You are not a participant of this chat room.");
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());
        _logger.LogInformation("User {UserId} joined room {RoomId}", userId, roomId);

        // Mark messages as read when joining
        await _chatService.MarkMessagesAsReadAsync(roomId, userId.Value);
    }

    public async Task LeaveRoom(Guid roomId)
    {
        var userId = GetUserId();
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId.ToString());
        _logger.LogInformation("User {UserId} left room {RoomId}", userId, roomId);
    }

    public async Task SendMessage(Guid roomId, string content)
    {
        var userId = GetUserId();
        if (!userId.HasValue)
        {
            await Clients.Caller.SendAsync("Error", "User not authenticated.");
            return;
        }

        try
        {
            // Persist message first (validates participant status)
            var messageResponse = await _chatService.SendMessageAsync(roomId, userId.Value, content);

            // Broadcast to all participants in the room group
            // Users are auto-joined to their room groups on connection
            await Clients.Group(roomId.ToString()).SendAsync("ReceiveMessage", messageResponse);

            _logger.LogInformation("User {UserId} sent message to room {RoomId}", userId, roomId);
        }
        catch (UnauthorizedAccessException ex)
        {
            await Clients.Caller.SendAsync("Error", ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            await Clients.Caller.SendAsync("Error", ex.Message);
        }
        catch (ArgumentException ex)
        {
            await Clients.Caller.SendAsync("Error", ex.Message);
        }
    }

    private Guid? GetUserId()
    {
        var userIdClaim = Context.User?.FindFirst("sub")?.Value 
                          ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }

        return null;
    }

    // Helper method to get connection IDs for a specific user (useful for direct notifications)
    public static IEnumerable<string> GetUserConnectionIds(Guid userId)
    {
        if (_userConnections.TryGetValue(userId, out var connections))
        {
            lock (connections)
            {
                return connections.ToList();
            }
        }
        return Enumerable.Empty<string>();
    }

    // Static method to add users to a room group (called from controllers when new chat is created)
    public static async Task AddUsersToRoomGroup(IHubContext<ChatHub> hubContext, Guid roomId, IEnumerable<Guid> userIds)
    {
        foreach (var userId in userIds)
        {
            var connectionIds = GetUserConnectionIds(userId);
            foreach (var connId in connectionIds)
            {
                await hubContext.Groups.AddToGroupAsync(connId, roomId.ToString());
            }
        }
    }
}
