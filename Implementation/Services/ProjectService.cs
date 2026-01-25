using Microsoft.EntityFrameworkCore;
using Global_Strategist_Platform_Server.Interface.Repositories;
using Global_Strategist_Platform_Server.Interface.Services;
using Global_Strategist_Platform_Server.Model.DTOs;
using Global_Strategist_Platform_Server.Model.Entities;
using Global_Strategist_Platform_Server.Model.Enum;
using Global_Strategist_Platform_Server.Gateway.FileManager;

namespace Global_Strategist_Platform_Server.Implementation.Services;

public class ProjectService : IProjectService
{
    private readonly IBaseRepository<Project> _projectRepository;
    private readonly IBaseRepository<Category> _categoryRepository;
    private readonly IFileManager _fileManager;

    public ProjectService(
        IBaseRepository<Project> projectRepository,
        IBaseRepository<Category> categoryRepository,
        IFileManager fileManager)
    {
        _projectRepository = projectRepository;
        _categoryRepository = categoryRepository;
        _fileManager = fileManager;
    }

    public async Task<ProjectDto?> GetByIdAsync(Guid id)
    {
        var project = await _projectRepository.Query()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

        return project == null ? null : MapToDto(project);
    }

    public async Task<IEnumerable<ProjectDto>> GetAllAsync()
    {
        var projects = await _projectRepository.Query()
            .Include(p => p.Category)
            .ToListAsync();

        return projects.Select(MapToDto);
    }

    public async Task<PagedResult<ProjectDto>> GetAllPagedAsync(PaginationRequest request)
    {
        var query = _projectRepository.Query()
            .Include(p => p.Category);

        var totalCount = await query.CountAsync();

        var projects = await query
            .OrderBy(p => p.Name)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return new PagedResult<ProjectDto>
        {
            Items = projects.Select(MapToDto),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<IEnumerable<ProjectDto>> GetByCategoryIdAsync(Guid categoryId)
    {
        var category = await _categoryRepository.GetByIdAsync(categoryId) ??
            throw new KeyNotFoundException($"Category with ID {categoryId} not found.");

        var projects = await _projectRepository.Query()
            .Include(p => p.Category)
            .Where(p => p.CategoryId == categoryId)
            .ToListAsync();

        return projects.Select(MapToDto);
    }

    public async Task<PagedResult<ProjectDto>> GetByCategoryIdPagedAsync(Guid categoryId, PaginationRequest request)
    {
        var category = await _categoryRepository.GetByIdAsync(categoryId) ??
            throw new KeyNotFoundException($"Category with ID {categoryId} not found.");

        var query = _projectRepository.Query()
            .Include(p => p.Category)
            .Where(p => p.CategoryId == categoryId);

        var totalCount = await query.CountAsync();

        var projects = await query
            .OrderBy(p => p.Name)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return new PagedResult<ProjectDto>
        {
            Items = projects.Select(MapToDto),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<ProjectDto> CreateAsync(CreateProjectDto createDto)
    {
        if (string.IsNullOrWhiteSpace(createDto.Name))
            throw new ArgumentException("Project name is required.", nameof(createDto));

        var category = await _categoryRepository.GetByIdAsync(createDto.CategoryId) ??
            throw new KeyNotFoundException($"Category with ID {createDto.CategoryId} not found.");

        string imageUrl = string.Empty;
        
        // Handle image upload
        if (createDto.Image != null)
        {
            var uploadResult = await _fileManager.UploadFile(createDto.Image, FileCategory.ProjectImage);
            if (uploadResult.success)
            {
                imageUrl = uploadResult.fileUrl;
            }
            else
            {
                throw new InvalidOperationException($"Failed to upload project image: {uploadResult.message}");
            }
        }

        var project = new Project
        {
            Id = Guid.NewGuid(),
            CategoryId = createDto.CategoryId,
            Name = createDto.Name.Trim(),
            Description = createDto.Description?.Trim() ?? string.Empty,
            ImageUrl = imageUrl,
            DateCreated = DateTime.UtcNow,
            IsDeleted = false
        };

        await _projectRepository.AddAsync(project);
        await _projectRepository.SaveChangesAsync();

        return MapToDto(project);
    }

    public async Task<ProjectDto> UpdateAsync(Guid id, UpdateProjectDto updateDto)
    {
        var project = await _projectRepository.GetByIdAsync(id) ??
            throw new KeyNotFoundException($"Project with ID {id} not found.");

        if (string.IsNullOrWhiteSpace(updateDto.Name))
            throw new ArgumentException("Project name is required.", nameof(updateDto));

        project.Name = updateDto.Name.Trim();
        project.Description = updateDto.Description?.Trim() ?? string.Empty;
        
        // Handle image upload if provided
        if (updateDto.Image != null)
        {
            var uploadResult = await _fileManager.UploadFile(updateDto.Image, FileCategory.ProjectImage);
            if (uploadResult.success)
            {
                // Delete old image if exists
                if (!string.IsNullOrEmpty(project.ImageUrl))
                {
                    await _fileManager.DeleteFile(project.ImageUrl, FileCategory.ProjectImage);
                }
                project.ImageUrl = uploadResult.fileUrl;
            }
            else
            {
                throw new InvalidOperationException($"Failed to upload project image: {uploadResult.message}");
            }
        }
        
        project.DateUpdated = DateTime.UtcNow;

        await _projectRepository.UpdateAsync(project);
        await _projectRepository.SaveChangesAsync();

        return MapToDto(project);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var project = await _projectRepository.GetByIdAsync(id);
        if (project == null)
            return false;

        await _projectRepository.DeleteAsync(project);
        await _projectRepository.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<ProjectWithFieldsDto>> GetAllWithFieldsAsync()
    {
        var projects = await _projectRepository.Query()
            .Include(p => p.Category)
            .Include(p => p.Fields)
            .ToListAsync();

        return projects.Select(MapToProjectWithFieldsDto);
    }

    private static ProjectDto MapToDto(Project project)
    {
        return new ProjectDto
        {
            Id = project.Id,
            CategoryId = project.CategoryId,
            Name = project.Name,
            Description = project.Description,
            ImageUrl = project.ImageUrl,
            DateCreated = project.DateCreated,
            DateUpdated = project.DateUpdated
        };
    }

    private static ProjectWithFieldsDto MapToProjectWithFieldsDto(Project project)
    {
        return new ProjectWithFieldsDto
        {
            Id = project.Id,
            CategoryId = project.CategoryId,
            Name = project.Name,
            Description = project.Description,
            ImageUrl = project.ImageUrl,
            DateCreated = project.DateCreated,
            DateUpdated = project.DateUpdated,
            Fields = project.Fields?.Select(f => new FieldDto
            {
                Id = f.Id,
                ProjectId = f.ProjectId,
                Name = f.Name,
                DateCreated = f.DateCreated,
                DateUpdated = f.DateUpdated
            }).ToList() ?? new List<FieldDto>()
        };
    }
}

