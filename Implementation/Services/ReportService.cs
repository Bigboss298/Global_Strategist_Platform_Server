using Microsoft.EntityFrameworkCore;
using Global_Strategist_Platform_Server.Interface.Repositories;
using Global_Strategist_Platform_Server.Interface.Services;
using Global_Strategist_Platform_Server.Model.DTOs;
using Global_Strategist_Platform_Server.Model.Entities;
using Global_Strategist_Platform_Server.Model.Enum;

namespace Global_Strategist_Platform_Server.Implementation.Services;

public class ReportService(
    IBaseRepository<Report> reportRepository,
    IBaseRepository<User> userRepository,
    IBaseRepository<Category> categoryRepository,
    IBaseRepository<Project> projectRepository,
    IBaseRepository<Field> fieldRepository,
    IReactionService reactionService) : IReportService
{
    private readonly IBaseRepository<Report> _reportRepository = reportRepository;
    private readonly IBaseRepository<User> _userRepository = userRepository;
    private readonly IBaseRepository<Category> _categoryRepository = categoryRepository;
    private readonly IBaseRepository<Project> _projectRepository = projectRepository;
    private readonly IBaseRepository<Field> _fieldRepository = fieldRepository;
    private readonly IReactionService _reactionService = reactionService;

    public async Task<ReportDto?> GetByIdAsync(Guid id)
    {
        var report = await _reportRepository.Query()
            .Include(r => r.Strategist)
            .Include(r => r.Category)
            .Include(r => r.Project)
            .Include(r => r.Field)
            .FirstOrDefaultAsync(r => r.Id == id);

        return report == null ? null : MapToDto(report);
    }

    public async Task<IEnumerable<ReportDto>> GetAllAsync()
    {
        var reports = await _reportRepository.Query()
            .Include(r => r.Strategist)
            .Include(r => r.Category)
            .Include(r => r.Project)
            .Include(r => r.Field)
            .ToListAsync();

        return reports.Select(MapToDto);
    }

    public async Task<IEnumerable<ReportFeedDto>> GetByStrategistIdAsync(Guid strategistId)
    {
        var strategist = await _userRepository.GetByIdAsync(strategistId) ??
            throw new KeyNotFoundException($"User (Strategist) with ID {strategistId} not found.");

        var reports = await _reportRepository.Query()
            .Include(r => r.Strategist)
            .Include(r => r.Category)
            .Include(r => r.Project)
            .Include(r => r.Field)
            .Include(r => r.Comments)
            .Include(r => r.Reactions)
            .Where(r => r.StrategistId == strategistId)
            .OrderByDescending(r => r.DateCreated)
            .ToListAsync();

        var feedItems = new List<ReportFeedDto>();

        foreach (var report in reports)
        {
            var reactionsSummary = await _reactionService.GetReactionsSummaryAsync(report.Id);

            feedItems.Add(new ReportFeedDto
            {
                Id = report.Id,
                Title = report.Title,
                Content = report.Content,
                StrategistId = report.StrategistId,
                StrategistName = report.Strategist?.FullName ?? string.Empty,
                StrategistFirstName = report.Strategist?.FirstName ?? string.Empty,
                StrategistLastName = report.Strategist?.LastName ?? string.Empty,
                StrategistCountry = report.Strategist?.Country ?? string.Empty,
                StrategistProfilePhotoUrl = report.Strategist?.ProfilePhotoUrl ?? string.Empty,
                CategoryName = report.Category?.Name ?? string.Empty,
                ProjectName = report.Project?.Name ?? string.Empty,
                FieldName = report.Field?.Name ?? string.Empty,
                ProjectImageUrl = report.Project?.ImageUrl ?? string.Empty,
                Images = report.Attachments ?? new List<string>(),
                CommentsCount = report.Comments.Count,
                ReactionsSummary = reactionsSummary,
                UserReaction = null,
                DateCreated = report.DateCreated,
                DateUpdated = report.DateUpdated
            });
        }

        return feedItems;
    }

    public async Task<IEnumerable<ReportDto>> GetByFieldIdAsync(Guid fieldId)
    {
        var field = await _fieldRepository.GetByIdAsync(fieldId) ??
            throw new KeyNotFoundException($"Field with ID {fieldId} not found.");

        var reports = await _reportRepository.Query()
            .Include(r => r.Strategist)
            .Include(r => r.Category)
            .Include(r => r.Project)
            .Include(r => r.Field)
            .Where(r => r.FieldId == fieldId)
            .ToListAsync();

        return reports.Select(MapToDto);
    }

    public async Task<IEnumerable<ReportDto>> GetByProjectIdAsync(Guid projectId)
    {
        var project = await _projectRepository.GetByIdAsync(projectId) ??
            throw new KeyNotFoundException($"Project with ID {projectId} not found.");

        var reports = await _reportRepository.Query()
            .Include(r => r.Strategist)
            .Include(r => r.Category)
            .Include(r => r.Project)
            .Include(r => r.Field)
            .Where(r => r.ProjectId == projectId)
            .ToListAsync();

        return reports.Select(MapToDto);
    }

    public async Task<IEnumerable<ReportDto>> GetByCategoryIdAsync(Guid categoryId)
    {
        var category = await _categoryRepository.GetByIdAsync(categoryId) ??
            throw new KeyNotFoundException($"Category with ID {categoryId} not found.");

        var reports = await _reportRepository.Query()
            .Include(r => r.Strategist)
            .Include(r => r.Category)
            .Include(r => r.Project)
            .Include(r => r.Field)
            .Where(r => r.CategoryId == categoryId)
            .ToListAsync();

        return reports.Select(MapToDto);
    }

    public async Task<IEnumerable<ReportFeedDto>> GetFeedAsync(Guid? userId = null)
    {
        var reports = await _reportRepository.Query()
            .Include(r => r.Strategist)
            .Include(r => r.Project)
            .Include(r => r.Category)
            .Include(r => r.Field)
            .Include(r => r.Comments)
            .Include(r => r.Reactions)
            .OrderByDescending(r => r.DateCreated)
            .ToListAsync();

        if (!reports.Any())
            return Enumerable.Empty<ReportFeedDto>();

        var reportIds = reports.Select(r => r.Id).ToList();

        // Batch operation: Get all reactions summaries in parallel
        var reactionsSummariesTask = Task.WhenAll(
            reportIds.Select(id => _reactionService.GetReactionsSummaryAsync(id))
        );

        // Batch operation: Get all user reactions if userId is provided
        Task<ReactionDto?>[] userReactionsTask = null;
        if (userId.HasValue)
        {
            userReactionsTask = reportIds
                .Select(id => _reactionService.GetByReportAndStrategistAsync(id, userId.Value))
                .ToArray();
        }

        // Wait for all batch operations to complete
        await reactionsSummariesTask;
        var reactionsSummaries = reactionsSummariesTask.Result;
        
        Dictionary<Guid, ReactionType?> userReactionsMap = null;
        if (userReactionsTask != null)
        {
            await Task.WhenAll(userReactionsTask);
            userReactionsMap = reportIds
                .Zip(userReactionsTask.Select(t => t.Result?.ReactionType), (id, reaction) => new { id, reaction })
                .ToDictionary(x => x.id, x => x.reaction);
        }

        // Map to DTOs using parallel processing
        var feedItems = reports
            .AsParallel()
            .AsOrdered()
            .Select((report, index) => new ReportFeedDto
            {
                Id = report.Id,
                Title = report.Title,
                Content = report.Content,
                StrategistId = report.StrategistId,
                StrategistName = report.Strategist?.FullName ?? string.Empty,
                StrategistFirstName = report.Strategist?.FirstName ?? string.Empty,
                StrategistLastName = report.Strategist?.LastName ?? string.Empty,
                StrategistCountry = report.Strategist?.Country ?? string.Empty,
                StrategistProfilePhotoUrl = report.Strategist?.ProfilePhotoUrl ?? string.Empty,
                BadgeType = (BadgeType)(report.Strategist?.BadgeType),
                CategoryName = report.Category?.Name ?? string.Empty,
                ProjectName = report.Project?.Name ?? string.Empty,
                FieldName = report.Field?.Name ?? string.Empty,
                ProjectImageUrl = report.Project?.ImageUrl ?? string.Empty,
                Images = report.Attachments ?? new List<string>(),
                CommentsCount = report.Comments.Count,
                ReactionsSummary = reactionsSummaries[index],
                UserReaction = userReactionsMap?[report.Id],
                DateCreated = report.DateCreated,
                DateUpdated = report.DateUpdated
            })
            .ToList();

        return feedItems;
    }

    public async Task<IEnumerable<ReportFeedDto>> GetFeedByProjectAsync(Guid projectId, Guid? userId = null)
    {
        // Verify project exists
        var project = await _projectRepository.GetByIdAsync(projectId);
        if (project == null)
            throw new KeyNotFoundException($"Project with ID {projectId} not found.");

        var reports = await _reportRepository.Query()
            .Include(r => r.Strategist)
            .Include(r => r.Project)
            .Include(r => r.Category)
            .Include(r => r.Field)
            .Include(r => r.Comments)
            .Include(r => r.Reactions)
            .Where(r => r.ProjectId == projectId)
            .OrderByDescending(r => r.DateCreated)
            .ToListAsync();

        if (!reports.Any())
            return Enumerable.Empty<ReportFeedDto>();

        var reportIds = reports.Select(r => r.Id).ToList();

        // Batch operation: Get all reactions summaries in parallel
        var reactionsSummariesTask = Task.WhenAll(
            reportIds.Select(id => _reactionService.GetReactionsSummaryAsync(id))
        );

        // Batch operation: Get all user reactions if userId is provided
        Task<ReactionDto?>[] userReactionsTask = null;
        if (userId.HasValue)
        {
            userReactionsTask = reportIds
                .Select(id => _reactionService.GetByReportAndStrategistAsync(id, userId.Value))
                .ToArray();
        }

        // Wait for all batch operations to complete
        await reactionsSummariesTask;
        var reactionsSummaries = reactionsSummariesTask.Result;
        
        Dictionary<Guid, ReactionType?> userReactionsMap = null;
        if (userReactionsTask != null)
        {
            await Task.WhenAll(userReactionsTask);
            userReactionsMap = reportIds
                .Zip(userReactionsTask.Select(t => t.Result?.ReactionType), (id, reaction) => new { id, reaction })
                .ToDictionary(x => x.id, x => x.reaction);
        }

        // Map to DTOs using parallel processing
        var feedItems = reports
            .AsParallel()
            .AsOrdered()
            .Select((report, index) => new ReportFeedDto
            {
                Id = report.Id,
                Title = report.Title,
                Content = report.Content,
                StrategistId = report.StrategistId,
                StrategistName = report.Strategist?.FullName ?? string.Empty,
                StrategistFirstName = report.Strategist?.FirstName ?? string.Empty,
                StrategistLastName = report.Strategist?.LastName ?? string.Empty,
                StrategistCountry = report.Strategist?.Country ?? string.Empty,
                StrategistProfilePhotoUrl = report.Strategist?.ProfilePhotoUrl ?? string.Empty,
                BadgeType = (BadgeType)(report.Strategist?.BadgeType),
                CategoryName = report.Category?.Name ?? string.Empty,
                ProjectName = report.Project?.Name ?? string.Empty,
                FieldName = report.Field?.Name ?? string.Empty,
                ProjectImageUrl = report.Project?.ImageUrl ?? string.Empty,
                Images = report.Attachments ?? new List<string>(),
                CommentsCount = report.Comments.Count,
                ReactionsSummary = reactionsSummaries[index],
                UserReaction = userReactionsMap?[report.Id],
                DateCreated = report.DateCreated,
                DateUpdated = report.DateUpdated
            })
            .ToList();

        return feedItems;
    }

    public async Task<ReportDto> CreateAsync(CreateReportDto createDto)
    {
        if (string.IsNullOrWhiteSpace(createDto.Title))
            throw new ArgumentException("Report title is required.", nameof(createDto));

        if (string.IsNullOrWhiteSpace(createDto.Content))
            throw new ArgumentException("Report content is required.", nameof(createDto));

        // Validate hierarchy
        var strategist = await _userRepository.GetByIdAsync(createDto.StrategistId) ??
            throw new KeyNotFoundException($"User (Strategist) with ID {createDto.StrategistId} not found.");

        if (!strategist.IsActive)
            throw new InvalidOperationException("User must be active to post reports.");

        if (string.IsNullOrWhiteSpace(strategist.Headline))
            throw new InvalidOperationException("User must have a headline to post reports.");

        var category = await _categoryRepository.GetByIdAsync(createDto.CategoryId) ??
            throw new KeyNotFoundException($"Category with ID {createDto.CategoryId} not found.");

        var project = await _projectRepository.Query()
            .FirstOrDefaultAsync(p => p.Id == createDto.ProjectId) ??
            throw new KeyNotFoundException($"Project with ID {createDto.ProjectId} not found.");

        if (project.CategoryId != createDto.CategoryId)
            throw new ArgumentException("Project does not belong to the specified category.");

        var field = await _fieldRepository.Query()
            .FirstOrDefaultAsync(f => f.Id == createDto.FieldId) ??
            throw new KeyNotFoundException($"Field with ID {createDto.FieldId} not found.");

        if (field.ProjectId != createDto.ProjectId)
            throw new ArgumentException("Field does not belong to the specified project.");

        var report = new Report
        {
            Id = Guid.NewGuid(),
            StrategistId = createDto.StrategistId,
            CategoryId = createDto.CategoryId,
            ProjectId = createDto.ProjectId,
            FieldId = createDto.FieldId,
            Title = createDto.Title.Trim(),
            Content = createDto.Content.Trim(),
            Attachments = createDto.Attachments ?? new List<string>(),
            Links = createDto.Links ?? new List<string>(),
            DateCreated = DateTime.UtcNow,
            IsDeleted = false
        };

        await _reportRepository.AddAsync(report);
        await _reportRepository.SaveChangesAsync();

        return MapToDto(report);
    }

    public async Task<ReportDto> UpdateAsync(Guid id, UpdateReportDto updateDto)
    {
        var report = await _reportRepository.GetByIdAsync(id) ??
            throw new KeyNotFoundException($"Report with ID {id} not found.");

        if (string.IsNullOrWhiteSpace(updateDto.Title))
            throw new ArgumentException("Report title is required.", nameof(updateDto));

        if (string.IsNullOrWhiteSpace(updateDto.Content))
            throw new ArgumentException("Report content is required.", nameof(updateDto));

        report.Title = updateDto.Title.Trim();
        report.Content = updateDto.Content.Trim();
        report.Attachments = updateDto.Attachments ?? new List<string>();
        report.Links = updateDto.Links ?? new List<string>();
        report.DateUpdated = DateTime.UtcNow;

        await _reportRepository.UpdateAsync(report);
        await _reportRepository.SaveChangesAsync();

        return MapToDto(report);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var report = await _reportRepository.GetByIdAsync(id);
        if (report == null)
            return false;

        await _reportRepository.DeleteAsync(report);
        await _reportRepository.SaveChangesAsync();
        return true;
    }

    private static ReportDto MapToDto(Report report)
    {
        return new ReportDto
        {
            Id = report.Id,
            StrategistId = report.StrategistId,
            CategoryId = report.CategoryId,
            ProjectId = report.ProjectId,
            FieldId = report.FieldId,
            Title = report.Title,
            Content = report.Content,
            Attachments = report.Attachments,
            Links = report.Links,
            DateCreated = report.DateCreated,
            DateUpdated = report.DateUpdated
        };
    }
    public async Task<PagedResult<ReportDto>> GetAllPagedAsync(PaginationRequest request)
    {
        var query = _reportRepository.Query()
            .Include(r => r.Strategist)
            .Include(r => r.Category)
            .Include(r => r.Project)
            .Include(r => r.Field);

        var totalCount = await query.CountAsync();

        var reports = await query
            .OrderByDescending(r => r.DateCreated)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return new PagedResult<ReportDto>
        {
            Items = reports.Select(MapToDto),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PagedResult<ReportDto>> GetByStrategistIdPagedAsync(Guid strategistId, PaginationRequest request)
    {
        var strategist = await _userRepository.GetByIdAsync(strategistId) ??
            throw new KeyNotFoundException($"User (Strategist) with ID {strategistId} not found.");

        var query = _reportRepository.Query()
            .Include(r => r.Strategist)
            .Include(r => r.Category)
            .Include(r => r.Project)
            .Include(r => r.Field)
            .Where(r => r.StrategistId == strategistId);

        var totalCount = await query.CountAsync();

        var reports = await query
            .OrderByDescending(r => r.DateCreated)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return new PagedResult<ReportDto>
        {
            Items = reports.Select(MapToDto),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PagedResult<ReportDto>> GetByFieldIdPagedAsync(Guid fieldId, PaginationRequest request)
    {
        var field = await _fieldRepository.GetByIdAsync(fieldId) ??
            throw new KeyNotFoundException($"Field with ID {fieldId} not found.");

        var query = _reportRepository.Query()
            .Include(r => r.Strategist)
            .Include(r => r.Category)
            .Include(r => r.Project)
            .Include(r => r.Field)
            .Where(r => r.FieldId == fieldId);

        var totalCount = await query.CountAsync();

        var reports = await query
            .OrderByDescending(r => r.DateCreated)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return new PagedResult<ReportDto>
        {
            Items = reports.Select(MapToDto),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PagedResult<ReportDto>> GetByProjectIdPagedAsync(Guid projectId, PaginationRequest request)
    {
        var project = await _projectRepository.GetByIdAsync(projectId) ??
            throw new KeyNotFoundException($"Project with ID {projectId} not found.");

        var query = _reportRepository.Query()
            .Include(r => r.Strategist)
            .Include(r => r.Category)
            .Include(r => r.Project)
            .Include(r => r.Field)
            .Where(r => r.ProjectId == projectId);

        var totalCount = await query.CountAsync();

        var reports = await query
            .OrderByDescending(r => r.DateCreated)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return new PagedResult<ReportDto>
        {
            Items = reports.Select(MapToDto),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PagedResult<ReportDto>> GetByCategoryIdPagedAsync(Guid categoryId, PaginationRequest request)
    {
        var category = await _categoryRepository.GetByIdAsync(categoryId) ??
            throw new KeyNotFoundException($"Category with ID {categoryId} not found.");

        var query = _reportRepository.Query()
            .Include(r => r.Strategist)
            .Include(r => r.Category)
            .Include(r => r.Project)
            .Include(r => r.Field)
            .Where(r => r.CategoryId == categoryId);

        var totalCount = await query.CountAsync();

        var reports = await query
            .OrderByDescending(r => r.DateCreated)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return new PagedResult<ReportDto>
        {
            Items = reports.Select(MapToDto),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PagedResult<ReportFeedDto>> GetFeedPagedAsync(Guid? userId, PaginationRequest request)
    {
        var query = _reportRepository.Query()
            .Include(r => r.Strategist)
            .Include(r => r.Project)
            .Include(r => r.Category)
            .Include(r => r.Field)
            .Include(r => r.Comments)
            .Include(r => r.Reactions)
            .OrderByDescending(r => r.DateCreated);

        var totalCount = await query.CountAsync();

        var reports = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        var feedItems = new List<ReportFeedDto>();

        foreach (var report in reports)
        {
            var reactionsSummary = await _reactionService.GetReactionsSummaryAsync(report.Id);

            ReactionType? userReaction = null;
            if (userId.HasValue)
            {
                var userReactionDto = await _reactionService.GetByReportAndStrategistAsync(report.Id, userId.Value);
                userReaction = userReactionDto?.ReactionType;
            }

            feedItems.Add(new ReportFeedDto
            {
                Id = report.Id,
                Title = report.Title,
                Content = report.Content,
                StrategistId = report.StrategistId,
                StrategistName = report.Strategist?.FullName ?? string.Empty,
                StrategistFirstName = report.Strategist?.FirstName ?? string.Empty,
                StrategistLastName = report.Strategist?.LastName ?? string.Empty,
                StrategistCountry = report.Strategist?.Country ?? string.Empty,
                StrategistProfilePhotoUrl = report.Strategist?.ProfilePhotoUrl ?? string.Empty,
                CategoryName = report.Category?.Name ?? string.Empty,
                ProjectName = report.Project?.Name ?? string.Empty,
                FieldName = report.Field?.Name ?? string.Empty,
                ProjectImageUrl = report.Project?.ImageUrl ?? string.Empty,
                Images = report.Attachments ?? new List<string>(),
                CommentsCount = report.Comments.Count,
                ReactionsSummary = reactionsSummary,
                UserReaction = userReaction,
                DateCreated = report.DateCreated,
                DateUpdated = report.DateUpdated
            });
        }

        return new PagedResult<ReportFeedDto>
        {
            Items = feedItems,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}

