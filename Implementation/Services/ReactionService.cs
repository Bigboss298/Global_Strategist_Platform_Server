using Microsoft.EntityFrameworkCore;
using Global_Strategist_Platform_Server.Interface.Repositories;
using Global_Strategist_Platform_Server.Interface.Services;
using Global_Strategist_Platform_Server.Model.DTOs;
using Global_Strategist_Platform_Server.Model.Entities;
using Global_Strategist_Platform_Server.Model.Enum;

namespace Global_Strategist_Platform_Server.Implementation.Services;

public class ReactionService(
    IBaseRepository<Reaction> reactionRepository,
    IBaseRepository<Report> reportRepository,
    IBaseRepository<User> userRepository) : IReactionService
{
    private readonly IBaseRepository<Reaction> _reactionRepository = reactionRepository;
    private readonly IBaseRepository<Report> _reportRepository = reportRepository;
    private readonly IBaseRepository<User> _userRepository = userRepository;

    public async Task<ReactionDto?> GetByReportAndStrategistAsync(Guid reportId, Guid strategistId)
    {
        var reaction = await _reactionRepository.Query()
            .Include(r => r.Strategist)
            .FirstOrDefaultAsync(r => r.ReportId == reportId && r.StrategistId == strategistId);

        return reaction == null ? null : MapToDto(reaction);
    }

    public async Task<IEnumerable<ReactionDto>> GetByReportIdAsync(Guid reportId)
    {
        var report = await _reportRepository.GetByIdAsync(reportId);
        if (report == null)
            throw new KeyNotFoundException($"Report with ID {reportId} not found.");

        var reactions = await _reactionRepository.Query()
            .Include(r => r.Strategist)
            .Where(r => r.ReportId == reportId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return reactions.Select(MapToDto);
    }

    public async Task<ReactionsSummaryDto> GetReactionsSummaryAsync(Guid reportId)
    {
        var report = await _reportRepository.GetByIdAsync(reportId) ??
            throw new KeyNotFoundException($"Report with ID {reportId} not found.");

        var reactions = await _reactionRepository.Query()
            .Where(r => r.ReportId == reportId)
            .ToListAsync();

        return new ReactionsSummaryDto
        {
            Like = reactions.Count(r => r.ReactionType == ReactionType.Like),
            Love = reactions.Count(r => r.ReactionType == ReactionType.Love),
            Insightful = reactions.Count(r => r.ReactionType == ReactionType.Insightful),
            Dislike = reactions.Count(r => r.ReactionType == ReactionType.Dislike)
        };
    }

    public async Task<ReactionDto> AddOrUpdateAsync(CreateReactionDto createDto)
    {
        var report = await _reportRepository.GetByIdAsync(createDto.ReportId) ??
            throw new KeyNotFoundException($"Report with ID {createDto.ReportId} not found.");

        var user = await _userRepository.GetByIdAsync(createDto.StrategistId) ??
            throw new KeyNotFoundException($"User (Strategist) with ID {createDto.StrategistId} not found.");

        // Check if reaction already exists
        var existingReaction = await _reactionRepository.Query()
            .Include(r => r.Strategist)
            .FirstOrDefaultAsync(r => r.ReportId == createDto.ReportId && r.StrategistId == createDto.StrategistId);

        if (existingReaction != null)
        {
            // Update existing reaction
            existingReaction.ReactionType = createDto.ReactionType;
            existingReaction.DateUpdated = DateTime.UtcNow;

            await _reactionRepository.UpdateAsync(existingReaction);
            await _reactionRepository.SaveChangesAsync();

            return MapToDto(existingReaction);
        }
        else
        {
            // Create new reaction
            var reaction = new Reaction
            {
                Id = Guid.NewGuid(),
                ReportId = createDto.ReportId,
                StrategistId = createDto.StrategistId,
                ReactionType = createDto.ReactionType,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await _reactionRepository.AddAsync(reaction);
            await _reactionRepository.SaveChangesAsync();

            // Reload with navigation properties
            var createdReaction = await _reactionRepository.Query()
                .Include(r => r.Strategist)
                .FirstOrDefaultAsync(r => r.Id == reaction.Id);

            return MapToDto(createdReaction!);
        }
    }

    public async Task<bool> RemoveAsync(Guid reportId, Guid strategistId)
    {
        var reaction = await _reactionRepository.Query()
            .FirstOrDefaultAsync(r => r.ReportId == reportId && r.StrategistId == strategistId);

        if (reaction == null)
            return false;

        await _reactionRepository.DeleteAsync(reaction);
        await _reactionRepository.SaveChangesAsync();
        return true;
    }

    private static ReactionDto MapToDto(Reaction reaction)
    {
        return new ReactionDto
        {
            Id = reaction.Id,
            ReportId = reaction.ReportId,
            StrategistId = reaction.StrategistId,
            StrategistName = reaction.Strategist?.FullName ?? string.Empty,
            ReactionType = reaction.ReactionType,
            CreatedAt = reaction.CreatedAt
        };
    }
}

