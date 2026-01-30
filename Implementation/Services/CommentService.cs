using Microsoft.EntityFrameworkCore;
using Global_Strategist_Platform_Server.Interface.Repositories;
using Global_Strategist_Platform_Server.Interface.Services;
using Global_Strategist_Platform_Server.Model.DTOs;
using Global_Strategist_Platform_Server.Model.Entities;

namespace Global_Strategist_Platform_Server.Implementation.Services;

public class CommentService(
    IBaseRepository<Comment> commentRepository,
    IBaseRepository<Report> reportRepository,
    IBaseRepository<User> userRepository) : ICommentService
{
    private readonly IBaseRepository<Comment> _commentRepository = commentRepository;
    private readonly IBaseRepository<Report> _reportRepository = reportRepository;
    private readonly IBaseRepository<User> _userRepository = userRepository;

    public async Task<CommentDto?> GetByIdAsync(Guid id)
    {
        var comment = await _commentRepository.Query()
            .Include(c => c.Strategist)
            .FirstOrDefaultAsync(c => c.Id == id);

        return comment == null ? null : MapToDto(comment);
    }

    public async Task<IEnumerable<CommentDto>> GetByReportIdAsync(Guid reportId)
    {
        var report = await _reportRepository.GetByIdAsync(reportId) ??
            throw new KeyNotFoundException($"Report with ID {reportId} not found.");

        var comments = await _commentRepository.Query()
            .Include(c => c.Strategist)
            .Where(c => c.ReportId == reportId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return comments.Select(MapToDto);
    }

    public async Task<CommentDto> CreateAsync(CreateCommentDto createDto)
    {
        if (string.IsNullOrWhiteSpace(createDto.Content))
            throw new ArgumentException("Comment content is required.", nameof(createDto));

        var report = await _reportRepository.GetByIdAsync(createDto.ReportId) ??
            throw new KeyNotFoundException($"Report with ID {createDto.ReportId} not found.");

        var user = await _userRepository.GetByIdAsync(createDto.StrategistId) ??
            throw new KeyNotFoundException($"User (Strategist) with ID {createDto.StrategistId} not found.");

        if (!user.IsActive)
            throw new InvalidOperationException("User must be active to post comments.");

        if (string.IsNullOrWhiteSpace(user.Headline))
            throw new InvalidOperationException("User must have a headline to post comments.");

        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            ReportId = createDto.ReportId,
            StrategistId = createDto.StrategistId,
            Content = createDto.Content.Trim(),
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        await _commentRepository.AddAsync(comment);
        await _commentRepository.SaveChangesAsync();

        // Reload with navigation properties
        var createdComment = await _commentRepository.Query()
            .Include(c => c.Strategist)
            .FirstOrDefaultAsync(c => c.Id == comment.Id);

        return MapToDto(createdComment!);
    }

    public async Task<CommentDto> UpdateAsync(Guid id, Guid strategistId, UpdateCommentDto updateDto)
    {
        if (string.IsNullOrWhiteSpace(updateDto.Content))
            throw new ArgumentException("Comment content is required.", nameof(updateDto));

        var comment = await _commentRepository.Query()
            .Include(c => c.Strategist)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (comment == null)
            throw new KeyNotFoundException($"Comment with ID {id} not found.");

        // Only comment owner can edit
        if (comment.StrategistId != strategistId)
            throw new UnauthorizedAccessException("Only the comment owner can edit this comment.");

        comment.Content = updateDto.Content.Trim();
        comment.UpdatedAt = DateTime.UtcNow;

        await _commentRepository.UpdateAsync(comment);
        await _commentRepository.SaveChangesAsync();

        return MapToDto(comment);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid strategistId)
    {
        var comment = await _commentRepository.GetByIdAsync(id);
        if (comment == null)
            return false;

        // Only comment owner can delete
        if (comment.StrategistId != strategistId)
            throw new UnauthorizedAccessException("Only the comment owner can delete this comment.");

        await _commentRepository.DeleteAsync(comment);
        await _commentRepository.SaveChangesAsync();
        return true;
    }

    private static CommentDto MapToDto(Comment comment)
    {
        return new CommentDto
        {
            Id = comment.Id,
            ReportId = comment.ReportId,
            StrategistId = comment.StrategistId,
            StrategistName = comment.Strategist?.FullName ?? string.Empty,
            StrategistProfilePhotoUrl = comment.Strategist?.ProfilePhotoUrl ?? string.Empty,
            BadgeType = comment.Strategist != null ? comment.Strategist.BadgeType : Global_Strategist_Platform_Server.Model.Enum.BadgeType.None,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt
        };
    }
}

