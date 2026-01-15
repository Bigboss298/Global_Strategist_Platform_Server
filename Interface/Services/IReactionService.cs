using Global_Strategist_Platform_Server.Model.DTOs;

namespace Global_Strategist_Platform_Server.Interface.Services;

public interface IReactionService
{
    Task<ReactionDto?> GetByReportAndStrategistAsync(Guid reportId, Guid strategistId);
    Task<IEnumerable<ReactionDto>> GetByReportIdAsync(Guid reportId);
    Task<ReactionsSummaryDto> GetReactionsSummaryAsync(Guid reportId);
    Task<ReactionDto> AddOrUpdateAsync(CreateReactionDto createDto);
    Task<bool> RemoveAsync(Guid reportId, Guid strategistId);
}

