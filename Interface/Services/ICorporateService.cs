using Global_Strategist_Platform_Server.Model.DTOs;

namespace Global_Strategist_Platform_Server.Interface.Services;

public interface ICorporateService
{
    Task<PurchaseSlotsResponse> PurchaseSlotsAsync(Guid corporateAccountId, PurchaseSlotsRequest request);
    Task<CorporateDashboardDto> GetDashboardAsync(Guid corporateAccountId);
    Task ValidateInviteCapacityAsync(Guid corporateAccountId);
    Task<PagedResult<CorporateAccountDto>> GetAllPagedAsync(PaginationRequest request);
}