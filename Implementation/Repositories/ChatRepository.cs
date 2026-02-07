using Microsoft.EntityFrameworkCore;
using Global_Strategist_Platform_Server.Data;
using Global_Strategist_Platform_Server.Interface.Repositories;
using Global_Strategist_Platform_Server.Model.Entities;
using Global_Strategist_Platform_Server.Model.Enum;

namespace Global_Strategist_Platform_Server.Implementation.Repositories;

public class ChatRepository : IChatRepository
{
    private readonly ApplicationDbContext _context;

    public ChatRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ChatRoom?> GetByIdAsync(Guid id)
    {
        return await _context.ChatRooms
            .AsNoTracking()
            .FirstOrDefaultAsync(cr => cr.Id == id);
    }

    public async Task<ChatRoom?> GetByIdWithParticipantsAsync(Guid id)
    {
        return await _context.ChatRooms
            .Include(cr => cr.Participants)
                .ThenInclude(p => p.User)
            .Include(cr => cr.Project)
            .AsNoTracking()
            .FirstOrDefaultAsync(cr => cr.Id == id);
    }

    public async Task<ChatRoom?> GetDirectChatRoomAsync(Guid userAId, Guid userBId)
    {
        // Find a direct chat room where both users are participants
        var chatRoom = await _context.ChatRooms
            .Include(cr => cr.Participants)
                .ThenInclude(p => p.User)
            .Where(cr => cr.RoomType == RoomType.Direct)
            .Where(cr => cr.Participants.Any(p => p.UserId == userAId) &&
                         cr.Participants.Any(p => p.UserId == userBId) &&
                         cr.Participants.Count == 2)
            .AsNoTracking()
            .FirstOrDefaultAsync();

        return chatRoom;
    }

    public async Task<ChatRoom?> GetProjectChatRoomAsync(Guid projectId)
    {
        return await _context.ChatRooms
            .Include(cr => cr.Participants)
                .ThenInclude(p => p.User)
            .Include(cr => cr.Project)
            .Where(cr => cr.RoomType == RoomType.Project && cr.ProjectId == projectId)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<ChatRoom>> GetUserChatRoomsAsync(Guid userId)
    {
        return await _context.ChatRooms
            .Include(cr => cr.Participants)
                .ThenInclude(p => p.User)
            .Include(cr => cr.Project)
            .Where(cr => cr.Participants.Any(p => p.UserId == userId))
            .OrderByDescending(cr => cr.DateCreated)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<ChatRoom> CreateAsync(ChatRoom chatRoom)
    {
        await _context.ChatRooms.AddAsync(chatRoom);
        return chatRoom;
    }

    public async Task AddParticipantAsync(ChatParticipant participant)
    {
        await _context.ChatParticipants.AddAsync(participant);
    }

    public async Task<bool> IsUserParticipantAsync(Guid roomId, Guid userId)
    {
        return await _context.ChatParticipants
            .AnyAsync(cp => cp.ChatRoomId == roomId && cp.UserId == userId);
    }

    public async Task<IEnumerable<Guid>> GetParticipantUserIdsAsync(Guid roomId)
    {
        return await _context.ChatParticipants
            .Where(cp => cp.ChatRoomId == roomId)
            .Select(cp => cp.UserId)
            .ToListAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
