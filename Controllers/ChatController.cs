using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Global_Strategist_Platform_Server.Interface.Services;
using Global_Strategist_Platform_Server.Model.DTOs;
using Global_Strategist_Platform_Server.Hubs;

namespace Global_Strategist_Platform_Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly IHubContext<ChatHub> _hubContext;

    public ChatController(IChatService chatService, IHubContext<ChatHub> hubContext)
    {
        _chatService = chatService;
        _hubContext = hubContext;
    }

    /// <summary>
    /// Get all chat rooms for the current user
    /// </summary>
    [HttpGet("rooms")]
    public async Task<ActionResult<IEnumerable<ChatRoomResponse>>> GetUserChatRooms()
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
            return Unauthorized("User not authenticated.");

        try
        {
            var rooms = await _chatService.GetUserChatRoomsAsync(userId.Value);
            return Ok(rooms);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Get or create a direct chat with another user
    /// </summary>
    [HttpPost("rooms/direct")]
    public async Task<ActionResult<ChatRoomResponse>> CreateOrGetDirectChat([FromBody] CreateDirectChatRequest request)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
            return Unauthorized("User not authenticated.");

        try
        {
            var room = await _chatService.CreateOrGetDirectChatAsync(userId.Value, request.OtherUserId);
            
            // Add both users to the SignalR group for this room
            var participantIds = new[] { userId.Value, request.OtherUserId };
            await ChatHub.AddUsersToRoomGroup(_hubContext, room.Id, participantIds);
            
            return Ok(room);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Get or create a project chat room
    /// </summary>
    [HttpPost("rooms/project")]
    public async Task<ActionResult<ChatRoomResponse>> CreateOrGetProjectChat([FromBody] CreateProjectChatRequest request)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
            return Unauthorized("User not authenticated.");

        try
        {
            var room = await _chatService.CreateOrGetProjectChatAsync(request.ProjectId, userId.Value);
            
            // Add the user to the SignalR group for this room
            await ChatHub.AddUsersToRoomGroup(_hubContext, room.Id, new[] { userId.Value });
            
            return Ok(room);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Get a specific chat room by ID
    /// </summary>
    [HttpGet("rooms/{roomId}")]
    public async Task<ActionResult<ChatRoomResponse>> GetChatRoom(Guid roomId)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
            return Unauthorized("User not authenticated.");

        try
        {
            var rooms = await _chatService.GetUserChatRoomsAsync(userId.Value);
            var room = rooms.FirstOrDefault(r => r.Id == roomId);

            if (room == null)
                return NotFound($"Chat room with ID {roomId} not found or you are not a participant.");

            return Ok(room);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Get paginated messages for a chat room
    /// </summary>
    [HttpGet("rooms/{roomId}/messages")]
    public async Task<ActionResult<ChatMessagesPagedResult>> GetMessages(
        Guid roomId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
            return Unauthorized("User not authenticated.");

        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        try
        {
            var messages = await _chatService.GetMessagesAsync(roomId, userId.Value, page, pageSize);
            return Ok(messages);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    /// <summary>
    /// Mark all messages in a room as read
    /// </summary>
    [HttpPost("rooms/{roomId}/read")]
    public async Task<ActionResult> MarkMessagesAsRead(Guid roomId)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
            return Unauthorized("User not authenticated.");

        try
        {
            await _chatService.MarkMessagesAsReadAsync(roomId, userId.Value);
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    /// <summary>
    /// Send a message to a chat room (REST alternative to SignalR)
    /// </summary>
    [HttpPost("rooms/{roomId}/messages")]
    public async Task<ActionResult<ChatMessageResponse>> SendMessage(Guid roomId, [FromBody] SendMessageRequest request)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
            return Unauthorized("User not authenticated.");

        try
        {
            var message = await _chatService.SendMessageAsync(roomId, userId.Value, request.Content);
            // If the client fell back to REST (SignalR disconnected), still deliver in real-time
            // to any connected participants listening to this room.
            await _hubContext.Clients.Group(roomId.ToString()).SendAsync("ReceiveMessage", message);
            return CreatedAtAction(nameof(GetMessages), new { roomId = roomId }, message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("sub")?.Value 
                          ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }

        return null;
    }
}
