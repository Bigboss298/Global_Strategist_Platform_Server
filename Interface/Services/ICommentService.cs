using Global_Strategist_Platform_Server.Model.DTOs;

namespace Global_Strategist_Platform_Server.Interface.Services;

public interface ICommentService
{
    Task<CommentDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<CommentDto>> GetByReportIdAsync(Guid reportId);
    Task<CommentDto> CreateAsync(CreateCommentDto createDto);
    Task<CommentDto> UpdateAsync(Guid id, Guid strategistId, UpdateCommentDto updateDto);
    Task<bool> DeleteAsync(Guid id, Guid strategistId);
}

