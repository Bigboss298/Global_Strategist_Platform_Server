using Global_Strategist_Platform_Server.Model.DTOs;

namespace Global_Strategist_Platform_Server.Interface.Services;

public interface IProjectService
{
    Task<ProjectDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<ProjectDto>> GetAllAsync();
    Task<IEnumerable<ProjectWithFieldsDto>> GetAllWithFieldsAsync();
    Task<PagedResult<ProjectDto>> GetAllPagedAsync(PaginationRequest request);
    Task<IEnumerable<ProjectDto>> GetByCategoryIdAsync(Guid categoryId);
    Task<PagedResult<ProjectDto>> GetByCategoryIdPagedAsync(Guid categoryId, PaginationRequest request);
    Task<ProjectDto> CreateAsync(CreateProjectDto createDto);
    Task<ProjectDto> UpdateAsync(Guid id, UpdateProjectDto updateDto);
    Task<bool> DeleteAsync(Guid id);
}

