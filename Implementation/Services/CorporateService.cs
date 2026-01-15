using Microsoft.EntityFrameworkCore;
using Global_Strategist_Platform_Server.Interface.Repositories;
using Global_Strategist_Platform_Server.Interface.Services;
using Global_Strategist_Platform_Server.Model.DTOs;
using Global_Strategist_Platform_Server.Model.Entities;
using Global_Strategist_Platform_Server.Model.Enum;

namespace Global_Strategist_Platform_Server.Implementation.Services;

public class CorporateService : ICorporateService
{
    private const decimal CostPerSlot = 2000m;
    private readonly IBaseRepository<CorporateAccount> _corporateAccountRepository;
    private readonly IBaseRepository<CorporatePayment> _corporatePaymentRepository;
    private readonly IBaseRepository<User> _userRepository;

    public CorporateService(
        IBaseRepository<CorporateAccount> corporateAccountRepository,
        IBaseRepository<CorporatePayment> corporatePaymentRepository,
        IBaseRepository<User> userRepository)
    {
        _corporateAccountRepository = corporateAccountRepository;
        _corporatePaymentRepository = corporatePaymentRepository;
        _userRepository = userRepository;
    }

    public async Task<PurchaseSlotsResponse> PurchaseSlotsAsync(Guid corporateAccountId, PurchaseSlotsRequest request)
    {
        var corporate = await _corporateAccountRepository.GetByIdAsync(corporateAccountId);
        if (corporate == null)
            throw new KeyNotFoundException("Corporate account not found.");

        // Calculate slots purchased
        var slotsPurchased = (int)(request.Amount / CostPerSlot);
        if (slotsPurchased == 0)
            throw new InvalidOperationException($"Amount must be at least {CostPerSlot} to purchase 1 slot.");

        // Create payment record
        var payment = new CorporatePayment
        {
            Id = Guid.NewGuid(),
            CorporateAccountId = corporateAccountId,
            Amount = request.Amount,
            SlotsPurchased = slotsPurchased,
            PaymentReference = request.PaymentReference,
            PaidAt = DateTime.UtcNow,
            DateCreated = DateTime.UtcNow,
            IsDeleted = false
        };

        await _corporatePaymentRepository.AddAsync(payment);

        // Update corporate account
        corporate.PaidMemberSlots += slotsPurchased;
        corporate.DateUpdated = DateTime.UtcNow;
        await _corporateAccountRepository.UpdateAsync(corporate);
        
        await _corporateAccountRepository.SaveChangesAsync();

        return new PurchaseSlotsResponse
        {
            SlotsPurchased = slotsPurchased,
            NewPaidMemberSlots = corporate.PaidMemberSlots,
            NewTotalSlots = corporate.TotalSlots,
            RemainingSlots = corporate.RemainingSlots,
            PaymentId = payment.Id
        };
    }

    public async Task<CorporateDashboardDto> GetDashboardAsync(Guid corporateAccountId)
    {
        var corporate = await _corporateAccountRepository.Query()
            .Include(c => c.Payments)
            .Include(c => c.Users)
            .FirstOrDefaultAsync(c => c.Id == corporateAccountId);

        if (corporate == null)
            throw new KeyNotFoundException("Corporate account not found.");

        var paymentHistory = corporate.Payments
            .OrderByDescending(p => p.PaidAt)
            .Select(p => new PaymentHistoryDto
            {
                Id = p.Id,
                Amount = p.Amount,
                SlotsPurchased = p.SlotsPurchased,
                PaymentReference = p.PaymentReference,
                PaidAt = p.PaidAt
            })
            .ToList();

        var teamMembers = corporate.Users
            .Where(u => u.Role == Role.CorporateTeam || u.Role == Role.CorporateAdmin)
            .OrderBy(u => u.DateCreated)
            .Select(u => new TeamMemberDto
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                Title = u.Title,
                DateCreated = u.DateCreated
            })
            .ToList();

        return new CorporateDashboardDto
        {
            CorporateAccountId = corporate.Id,
            OrganisationName = corporate.OrganisationName,
            FreeMemberSlots = CorporateAccount.FreeMemberSlots,
            PaidMemberSlots = corporate.PaidMemberSlots,
            UsedMemberSlots = corporate.UsedMemberSlots,
            TotalSlots = corporate.TotalSlots,
            RemainingSlots = corporate.RemainingSlots,
            PaymentHistory = paymentHistory,
            TeamMembers = teamMembers
        };
    }

    public async Task ValidateInviteCapacityAsync(Guid corporateAccountId)
    {
        var corporate = await _corporateAccountRepository.GetByIdAsync(corporateAccountId);
        if (corporate == null)
            throw new KeyNotFoundException("Corporate account not found.");

        if (corporate.RemainingSlots <= 0)
            throw new InvalidOperationException("Upgrade Required: No remaining member slots available. Please purchase additional slots to invite more team members.");
    }

    public async Task<PagedResult<CorporateAccountDto>> GetAllPagedAsync(PaginationRequest request)
    {
        var query = _corporateAccountRepository.Query()
            .Where(c => !c.IsDeleted)
            .OrderByDescending(c => c.DateCreated);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new CorporateAccountDto
            {
                Id = c.Id,
                OrganisationName = c.OrganisationName,
                Sector = c.Sector,
                Country = c.Country,
                DateCreated = c.DateCreated,    
                ContactEmail = c.RepresentativeEmail,
                ContactPhone = c.PhoneNumber
            })
            .ToListAsync();

        return new PagedResult<CorporateAccountDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}