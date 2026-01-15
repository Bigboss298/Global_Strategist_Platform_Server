using System.Security.Cryptography;
using System.Text;
using Global_Strategist_Platform_Server.Interface.Repositories;
using Global_Strategist_Platform_Server.Interface.Services;
using Global_Strategist_Platform_Server.Model.DTOs;
using Global_Strategist_Platform_Server.Model.Entities;
using Microsoft.EntityFrameworkCore;

namespace Global_Strategist_Platform_Server.Implementation.Services;

public class UserService : IUserService
{
    private readonly IBaseRepository<User> _userRepository;

    public UserService(IBaseRepository<User> userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto?> GetByIdAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        return user == null ? null : MapToDto(user);
    }

    public async Task<UserDto?> GetByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null or empty.", nameof(email));

        var user = await _userRepository.Query()
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

        return user == null ? null : MapToDto(user);
    }

    public async Task<IEnumerable<UserDto>> GetAllAsync()
    {
        var users = await _userRepository.Query().ToListAsync();
        return users.Select(MapToDto);
    }

    public async Task<PagedResult<UserDto>> GetAllPagedAsync(PaginationRequest request)
    {
        var query = _userRepository.Query();

        var totalCount = await query.CountAsync();

        var users = await query
            .Where(x => x.Role != Model.Enum.Role.Admin)
            .OrderBy(u => u.FullName)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return new PagedResult<UserDto>
        {
            Items = users.Select(MapToDto),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<UserDto> UpdateAsync(Guid id, UpdateUserDto updateDto)
    {
        var user = await _userRepository.GetByIdAsync(id) ??
            throw new KeyNotFoundException($"User with ID {id} not found.");

        if (string.IsNullOrWhiteSpace(updateDto.FirstName) && string.IsNullOrWhiteSpace(updateDto.LastName))
            throw new ArgumentException("User full name is required.", nameof(updateDto));

        user.FullName = $"{updateDto.FirstName} {updateDto.LastName}".Trim();
        user.FirstName = updateDto.FirstName?.Trim() ?? string.Empty;
        user.LastName = updateDto.LastName?.Trim() ?? string.Empty;
        user.Headline = updateDto.Headline?.Trim() ?? string.Empty;
        user.Country = updateDto.Country?.Trim() ?? string.Empty;
        user.ProfilePhotoUrl = updateDto.ProfilePhotoUrl?.Trim() ?? string.Empty;
        user.Certification = updateDto.Certification?.Trim() ?? string.Empty;
        user.Title = updateDto.Title?.Trim() ?? string.Empty;
        user.ShortBio = updateDto.ShortBio?.Trim() ?? string.Empty;
        user.CvFileUrl = updateDto.CvFileUrl?.Trim() ?? string.Empty;
        user.DateUpdated = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        return MapToDto(user);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            return false;

        await _userRepository.DeleteAsync(user);
        await _userRepository.SaveChangesAsync();
        return true;
    }
    private static UserDto MapToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Headline = user.Headline,
            Country = user.Country,
            ProfilePhotoUrl = user.ProfilePhotoUrl,
            Certification = user.Certification,
            Title = user.Title,
            ShortBio = user.ShortBio,
            CvFileUrl = user.CvFileUrl,
            IsActive = user.IsActive,
            Role = user.Role,
            CorporateAccountId = user.CorporateAccountId,
            DateCreated = user.DateCreated,
            DateUpdated = user.DateUpdated
        };
    }
}

