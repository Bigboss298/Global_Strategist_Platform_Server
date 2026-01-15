using Global_Strategist_Platform_Server.Model.DTOs;
using Global_Strategist_Platform_Server.Model.Entities;

namespace Global_Strategist_Platform_Server.Interface.Services;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginDto dto, string ipAddress);
    Task<AuthResponseDto> RefreshTokenAsync(string token, string ipAddress);
    Task RevokeRefreshTokenAsync(string token, string ipAddress);
    Task<UserDto?> GetCurrentUserAsync(Guid userId);

    // ✅ CHANGED: Return type from RegisterIndividualResponse to AuthResponseDto
    Task<AuthResponseDto> RegisterIndividualAsync(RegisterIndividualRequest dto, string ipAddress);

    // Corporate registration and invites
    Task<AuthResponseDto> RegisterCorporateAsync(RegisterCorporateRequest dto, string ipAddress);
    Task<CorporateInvite> InviteTeamMemberAsync(InviteTeamMemberRequest dto);
    Task<AuthResponseDto> RegisterTeamMemberAsync(RegisterTeamMemberRequest dto, string ipAddress);
}

