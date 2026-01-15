using Global_Strategist_Platform_Server.Model.Entities;

namespace Global_Strategist_Platform_Server.Model.Entities;

public class CorporateInvite : BaseEntity
{
    public Guid CorporateAccountId { get; set; }
    public CorporateAccount CorporateAccount { get; set; } = null!;

    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty; // GUID string
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
}
