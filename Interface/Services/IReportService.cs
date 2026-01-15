using Global_Strategist_Platform_Server.Model.DTOs;

namespace Global_Strategist_Platform_Server.Interface.Services;

public interface IReportService
{
    Task<ReportDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<ReportDto>> GetAllAsync();
    Task<PagedResult<ReportDto>> GetAllPagedAsync(PaginationRequest request);
    Task<IEnumerable<ReportFeedDto>> GetByStrategistIdAsync(Guid strategistId);
    Task<PagedResult<ReportDto>> GetByStrategistIdPagedAsync(Guid strategistId, PaginationRequest request);
    Task<IEnumerable<ReportDto>> GetByFieldIdAsync(Guid fieldId);
    Task<PagedResult<ReportDto>> GetByFieldIdPagedAsync(Guid fieldId, PaginationRequest request);
    Task<IEnumerable<ReportDto>> GetByProjectIdAsync(Guid projectId);
    Task<PagedResult<ReportDto>> GetByProjectIdPagedAsync(Guid projectId, PaginationRequest request);
    Task<IEnumerable<ReportDto>> GetByCategoryIdAsync(Guid categoryId);
    Task<PagedResult<ReportDto>> GetByCategoryIdPagedAsync(Guid categoryId, PaginationRequest request);
    Task<IEnumerable<ReportFeedDto>> GetFeedAsync(Guid? userId = null);
    Task<PagedResult<ReportFeedDto>> GetFeedPagedAsync(Guid? userId, PaginationRequest request);
    Task<ReportDto> CreateAsync(CreateReportDto createDto);
    Task<ReportDto> UpdateAsync(Guid id, UpdateReportDto updateDto);
    Task<bool> DeleteAsync(Guid id);
}

