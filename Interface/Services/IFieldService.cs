using Global_Strategist_Platform_Server.Model.DTOs;

namespace Global_Strategist_Platform_Server.Interface.Services;

public interface IFieldService
{
    Task<FieldDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<FieldDto>> GetAllAsync();
    Task<IEnumerable<FieldDto>> GetByProjectIdAsync(Guid projectId);
    Task<FieldDto> CreateAsync(CreateFieldDto createDto);
    Task<FieldDto> UpdateAsync(Guid id, UpdateFieldDto updateDto);
    Task<bool> DeleteAsync(Guid id);
}

