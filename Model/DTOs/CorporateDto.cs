using System.ComponentModel.DataAnnotations;

namespace Global_Strategist_Platform_Server.Model.DTOs;

public class PurchaseSlotsRequest
{
    [Required]
    [Range(2000, double.MaxValue, ErrorMessage = "Minimum purchase amount is 2000")]
    public decimal Amount { get; set; }

    [Required]
    [StringLength(200)]
    public string PaymentReference { get; set; } = string.Empty;
}

public class PurchaseSlotsResponse
{
    public int SlotsPurchased { get; set; }
    public int NewPaidMemberSlots { get; set; }
    public int NewTotalSlots { get; set; }
    public int RemainingSlots { get; set; }
    public Guid PaymentId { get; set; }
}

public class CorporateDashboardDto
{
    public Guid CorporateAccountId { get; set; }
    public string OrganisationName { get; set; } = string.Empty;
    public int FreeMemberSlots { get; set; }
    public int PaidMemberSlots { get; set; }
    public int UsedMemberSlots { get; set; }
    public int TotalSlots { get; set; }
    public int RemainingSlots { get; set; }
    public List<PaymentHistoryDto> PaymentHistory { get; set; } = new();
    public List<TeamMemberDto> TeamMembers { get; set; } = new();
}

public class PaymentHistoryDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public int SlotsPurchased { get; set; }
    public string PaymentReference { get; set; } = string.Empty;
    public DateTime PaidAt { get; set; }
}

public class TeamMemberDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime DateCreated { get; set; }
}

public class CorporateAccountDto
{
    public Guid Id { get; set; }
    public string OrganisationName { get; set; } = string.Empty;
    public string Sector { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
    //public int FreeMemberSlots { get; set; }
    //public int PaidMemberSlots { get; set; }
    //public int UsedMemberSlots { get; set; }
    //public int TotalSlots { get; set; }
    //public int RemainingSlots { get; set; }
    public DateTime DateCreated { get; set; }
}