using Global_Strategist_Platform_Server.Model.Entities;

namespace Global_Strategist_Platform_Server.Model.Entities;

public class CorporatePayment : BaseEntity
{
    public Guid CorporateAccountId { get; set; }
    public CorporateAccount CorporateAccount { get; set; } = null!;
    
    public decimal Amount { get; set; }
    public int SlotsPurchased { get; set; }
    public string PaymentReference { get; set; } = string.Empty;
    public DateTime PaidAt { get; set; }
}