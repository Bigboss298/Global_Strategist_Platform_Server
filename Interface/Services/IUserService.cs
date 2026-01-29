using Global_Strategist_Platform_Server.Model.DTOs;

namespace Global_Strategist_Platform_Server.Interface.Services;

public interface IUserService
{
    Task<UserDto?> GetByIdAsync(Guid id);
    Task<UserDto?> GetByEmailAsync(string email);
    Task<IEnumerable<UserDto>> GetAllAsync();
    Task<PagedResult<UserDto>> GetAllPagedAsync(PaginationRequest request);
    Task<UserDto> UpdateAsync(Guid id, UpdateUserDto updateDto);
    Task<bool> DeleteAsync(Guid id);
    Task<UserDto> UpdateBadgeAsync(Guid id, UpdateBadgeDto updateBadgeDto);
}

