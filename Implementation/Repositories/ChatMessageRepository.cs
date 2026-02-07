using Microsoft.EntityFrameworkCore;
using Global_Strategist_Platform_Server.Data;
using Global_Strategist_Platform_Server.Interface.Repositories;
using Global_Strategist_Platform_Server.Model.Entities;

namespace Global_Strategist_Platform_Server.Implementation.Repositories;

public class ChatMessageRepository : IChatMessageRepository
{
    private readonly ApplicationDbContext _context;

    public ChatMessageRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ChatMessage?> GetByIdAsync(Guid id)
    {
        return await _context.ChatMessages
            .Include(cm => cm.Sender)
            .AsNoTracking()
            .FirstOrDefaultAsync(cm => cm.Id == id);
    }

    public async Task<IEnumerable<ChatMessage>> GetByRoomIdAsync(Guid roomId, int page, int pageSize)
    {
        return await _context.ChatMessages
            .Include(cm => cm.Sender)
            .Where(cm => cm.ChatRoomId == roomId)
            .OrderByDescending(cm => cm.DateCreated)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<int> GetMessageCountAsync(Guid roomId)
    {
        return await _context.ChatMessages
            .Where(cm => cm.ChatRoomId == roomId)
            .CountAsync();
    }

    public async Task<ChatMessage?> GetLastMessageAsync(Guid roomId)
    {
        return await _context.ChatMessages
            .Include(cm => cm.Sender)
            .Where(cm => cm.ChatRoomId == roomId)
            .OrderByDescending(cm => cm.DateCreated)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public async Task<int> GetUnreadCountAsync(Guid roomId, Guid userId)
    {
        return await _context.ChatMessages
            .Where(cm => cm.ChatRoomId == roomId && cm.SenderId != userId && !cm.IsRead)
            .CountAsync();
    }

    public async Task<ChatMessage> CreateAsync(ChatMessage message)
    {
        await _context.ChatMessages.AddAsync(message);
        return message;
    }

    public async Task MarkMessagesAsReadAsync(Guid roomId, Guid userId)
    {
        var unreadMessages = await _context.ChatMessages
            .Where(cm => cm.ChatRoomId == roomId && cm.SenderId != userId && !cm.IsRead)
            .ToListAsync();

        foreach (var message in unreadMessages)
        {
            message.IsRead = true;
        }
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
