using Global_Strategist_Platform_Server.Model.DTOs;

namespace Global_Strategist_Platform_Server.Interface.Services;

public interface ICategoryService
{
    Task<CategoryDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<CategoryDto>> GetAllAsync();
    Task<CategoryDto> CreateAsync(CreateCategoryDto createDto);
    Task<CategoryDto> UpdateAsync(Guid id, UpdateCategoryDto updateDto);
    Task<bool> DeleteAsync(Guid id);
}

