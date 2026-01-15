using Global_Strategist_Platform_Server.Interface.Repositories;
using Global_Strategist_Platform_Server.Interface.Services;
using Global_Strategist_Platform_Server.Model.DTOs;
using Global_Strategist_Platform_Server.Model.Entities;

namespace Global_Strategist_Platform_Server.Implementation.Services;

public class CategoryService(IBaseRepository<Category> repository) : ICategoryService
{
    private readonly IBaseRepository<Category> _repository = repository;

    public async Task<CategoryDto?> GetByIdAsync(Guid id)
    {
        var category = await _repository.GetByIdAsync(id);
        return category == null ? null : MapToDto(category);
    }

    public async Task<IEnumerable<CategoryDto>> GetAllAsync()
    {
        var categories = await _repository.GetAllAsync();
        return categories.Select(MapToDto);
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto createDto)
    {
        if (string.IsNullOrWhiteSpace(createDto.Name))
            throw new ArgumentException("Category name is required.", nameof(createDto));

        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = createDto.Name.Trim(),
            Description = createDto.Description?.Trim() ?? string.Empty,
            DateCreated = DateTime.UtcNow,
            IsDeleted = false
        };

        await _repository.AddAsync(category);
        await _repository.SaveChangesAsync();

        return MapToDto(category);
    }

    public async Task<CategoryDto> UpdateAsync(Guid id, UpdateCategoryDto updateDto)
    {
        var category = await _repository.GetByIdAsync(id) ??
            throw new KeyNotFoundException($"Category with ID {id} not found.");

        if (string.IsNullOrWhiteSpace(updateDto.Name))
            throw new ArgumentException("Category name is required.", nameof(updateDto));

        category.Name = updateDto.Name.Trim();
        category.Description = updateDto.Description?.Trim() ?? string.Empty;
        category.DateUpdated = DateTime.UtcNow;

        await _repository.UpdateAsync(category);
        await _repository.SaveChangesAsync();

        return MapToDto(category);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var category = await _repository.GetByIdAsync(id);
        if (category == null)
            return false;

        await _repository.DeleteAsync(category);
        await _repository.SaveChangesAsync();
        return true;
    }

    private static CategoryDto MapToDto(Category category)
    {
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            DateCreated = category.DateCreated,
            DateUpdated = category.DateUpdated
        };
    }
}

