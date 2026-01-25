using System.ComponentModel.DataAnnotations;
using Global_Strategist_Platform_Server.Model.Enum;

namespace Global_Strategist_Platform_Server.Model.DTOs;

public class RegisterDto
{
    // Kept for backward compatibility - default strategist registration replaced by RegisterIndividualDto later
    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 8)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$", 
        ErrorMessage = "Password must be at least 8 characters and contain uppercase, lowercase, number, and special character")]
    public string Password { get; set; } = string.Empty;
}

public class LoginDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class AuthResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
    public UserDto User { get; set; } = null!;
}

public class RefreshRequestDto
{
    [Required]
    public string Token { get; set; } = string.Empty;
}

public class RevokeTokenRequestDto
{
    [Required]
    public string Token { get; set; } = string.Empty;
}

// New DTOs for individual registration
public class RegisterIndividualRequest
{
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Certification { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string ShortBio { get; set; } = string.Empty;

    public IFormFile? CvFile { get; set; }

    [Required]
    [StringLength(100)]
    public string Country { get; set; } = string.Empty;
}
// DTOs for corporate registration
public class RegisterCorporateRequest
{
    [Required]
    public string OrganisationName { get; set; } = string.Empty;

    [Required]
    public string RepresentativeFirstName { get; set; } = string.Empty;

    [Required]
    public string RepresentativeLastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string RepresentativeEmail { get; set; } = string.Empty;

    [Required]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    public string Country { get; set; } = string.Empty;

    [Required]
    public string Sector { get; set; } = string.Empty;

    [Required]
    [StringLength(250, MinimumLength = 150)]
    public string CompanyOverview { get; set; } = string.Empty;

    public List<string> ContributionInterestAreas { get; set; } = new();
    public List<string> SupportingDocuments { get; set; } = new();
    public string? OptionalNotes { get; set; }
    [Required]
    [StringLength(250, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;
    public bool DeclarationAccepted { get; set; }
}

public class RegisterCorporateResponse
{
    public Guid CorporateAccountId { get; set; }
    public Guid RepresentativeUserId { get; set; }
}

public class InviteTeamMemberRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public Guid CorporateAccountId { get; set; }
}

public class RegisterTeamMemberRequest
{
    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string AreaOfExpertise { get; set; } = string.Empty;

    public string? Certification { get; set; }
    public string? ShortBio { get; set; }
    public IFormFile? CvFile { get; set; }

    [Required]
    [StringLength(100)]
    public string Country { get; set; } = string.Empty;

    [Required]
    public string Token { get; set; } = string.Empty; // invite token
}

