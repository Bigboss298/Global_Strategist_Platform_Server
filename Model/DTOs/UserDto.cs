using Global_Strategist_Platform_Server.Model.Enum;

namespace Global_Strategist_Platform_Server.Model.DTOs;

public class UserDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Headline { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string ProfilePhotoUrl { get; set; } = string.Empty;
    public string Certification { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string ShortBio { get; set; } = string.Empty;
    public string CvFileUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public Role Role { get; set; }
    public Guid? CorporateAccountId { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime? DateUpdated { get; set; }
}

public class CreateUserDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Headline { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public IFormFile? ProfilePhoto { get; set; }
}

public class UpdateUserDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Headline { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public IFormFile? ProfilePhoto { get; set; }
    public string Certification { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string ShortBio { get; set; } = string.Empty;
    public IFormFile? CvFile { get; set; }
    public bool IsActive { get; set; }
}

