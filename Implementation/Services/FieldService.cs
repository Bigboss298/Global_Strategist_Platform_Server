using Microsoft.EntityFrameworkCore;
using Global_Strategist_Platform_Server.Interface.Repositories;
using Global_Strategist_Platform_Server.Interface.Services;
using Global_Strategist_Platform_Server.Model.DTOs;
using Global_Strategist_Platform_Server.Model.Entities;

namespace Global_Strategist_Platform_Server.Implementation.Services;

public class FieldService(
    IBaseRepository<Field> fieldRepository,
    IBaseRepository<Project> projectRepository) : IFieldService
{
    private readonly IBaseRepository<Field> _fieldRepository = fieldRepository;
    private readonly IBaseRepository<Project> _projectRepository = projectRepository;

    public async Task<FieldDto?> GetByIdAsync(Guid id)
    {
        var field = await _fieldRepository.Query()
            .Include(f => f.Project)
            .FirstOrDefaultAsync(f => f.Id == id);

        return field == null ? null : MapToDto(field);
    }

    public async Task<IEnumerable<FieldDto>> GetAllAsync()
    {
        var fields = await _fieldRepository.Query()
            .Include(f => f.Project)
            .ToListAsync();

        return fields.Select(MapToDto);
    }

    public async Task<IEnumerable<FieldDto>> GetByProjectIdAsync(Guid projectId)
    {
        var project = await _projectRepository.GetByIdAsync(projectId);
        if (project == null)
            throw new KeyNotFoundException($"Project with ID {projectId} not found.");

        var fields = await _fieldRepository.Query()
            .Include(f => f.Project)
            .Where(f => f.ProjectId == projectId)
            .ToListAsync();

        return fields.Select(MapToDto);
    }

    public async Task<FieldDto> CreateAsync(CreateFieldDto createDto)
    {
        if (string.IsNullOrWhiteSpace(createDto.Name))
            throw new ArgumentException("Field name is required.", nameof(createDto));

        var project = await _projectRepository.GetByIdAsync(createDto.ProjectId) ??
            throw new KeyNotFoundException($"Project with ID {createDto.ProjectId} not found.");

        var field = new Field
        {
            Id = Guid.NewGuid(),
            ProjectId = createDto.ProjectId,
            Name = createDto.Name.Trim(),
            DateCreated = DateTime.UtcNow,
            IsDeleted = false
        };

        await _fieldRepository.AddAsync(field);
        await _fieldRepository.SaveChangesAsync();

        return MapToDto(field);
    }

    public async Task<FieldDto> UpdateAsync(Guid id, UpdateFieldDto updateDto)
    {
        var field = await _fieldRepository.GetByIdAsync(id) ??
            throw new KeyNotFoundException($"Field with ID {id} not found.");

        if (string.IsNullOrWhiteSpace(updateDto.Name))
            throw new ArgumentException("Field name is required.", nameof(updateDto));

        field.Name = updateDto.Name.Trim();

        if (updateDto.ProjectId.HasValue && updateDto.ProjectId.Value != field.ProjectId)
        {
            var project = await _projectRepository.GetByIdAsync(updateDto.ProjectId.Value);
            if (project == null)
                throw new KeyNotFoundException($"Project with ID {updateDto.ProjectId.Value} not found.");

            field.ProjectId = updateDto.ProjectId.Value;
        }

        field.DateUpdated = DateTime.UtcNow;

        await _fieldRepository.UpdateAsync(field);
        await _fieldRepository.SaveChangesAsync();

        return MapToDto(field);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var field = await _fieldRepository.GetByIdAsync(id);
        if (field == null)
            return false;

        await _fieldRepository.DeleteAsync(field);
        await _fieldRepository.SaveChangesAsync();
        return true;
    }

    private static FieldDto MapToDto(Field field)
    {
        return new FieldDto
        {
            Id = field.Id,
            ProjectId = field.ProjectId,
            Name = field.Name,
            DateCreated = field.DateCreated,
            DateUpdated = field.DateUpdated
        };
    }
}

