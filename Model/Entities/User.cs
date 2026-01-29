using Global_Strategist_Platform_Server.Model.Entities;
using Global_Strategist_Platform_Server.Model.Enum;

namespace Global_Strategist_Platform_Server.Model.Entities;

public class User : BaseEntity
{
    // Existing fields
    public string FullName { get; set; } = string.Empty;

    // New individual name split
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Headline { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string ProfilePhotoUrl { get; set; } = string.Empty;

    // New profile fields
    public string Certification { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string ShortBio { get; set; } = string.Empty;
    public string CvFileUrl { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
    public Role Role { get; set; } = Role.Strategist;

    // Verification Badge fields
    public bool IsVerified { get; set; } = false;
    public BadgeType BadgeType { get; set; } = BadgeType.None;
    public DateTime? VerifiedDate { get; set; }
    public string? VerificationNote { get; set; } // Optional note about verification

    // Link to corporate account if user is part of corporate team
    public Guid? CorporateAccountId { get; set; }
    public CorporateAccount? CorporateAccount { get; set; }

    // Navigation properties
    public ICollection<Report> Reports { get; set; } = new List<Report>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}

