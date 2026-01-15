using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Global_Strategist_Platform_Server.Interface.Repositories;
using Global_Strategist_Platform_Server.Interface.Services;
using Global_Strategist_Platform_Server.Model.DTOs;
using Global_Strategist_Platform_Server.Model.Entities;
using Global_Strategist_Platform_Server.Model.Enum;
using Global_Strategist_Platform_Server.Gateway.EmailSender;

namespace Global_Strategist_Platform_Server.Implementation.Services;

public class AuthService : IAuthService
{
    private readonly IBaseRepository<User> _userRepository;
    private readonly IBaseRepository<RefreshToken> _refreshTokenRepository;
    private readonly IBaseRepository<CorporateAccount> _corporateAccountRepository;
    private readonly IBaseRepository<CorporateInvite> _corporateInviteRepository;
    private readonly ICorporateService _corporateService;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    public AuthService(
        IBaseRepository<User> userRepository,
        IBaseRepository<RefreshToken> refreshTokenRepository,
        IBaseRepository<CorporateAccount> corporateAccountRepository,
        IBaseRepository<CorporateInvite> corporateInviteRepository,
        ICorporateService corporateService,
        IEmailService emailService,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _corporateAccountRepository = corporateAccountRepository;
        _corporateInviteRepository = corporateInviteRepository;
        _corporateService = corporateService;
        _emailService = emailService;
        _configuration = configuration;
    }

    //public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto, string ipAddress)
    //{
    //    var existingUser = await _userRepository.FirstOrDefaultAsync(u => u.Email == dto.Email);
    //    if (existingUser != null)
    //        throw new InvalidOperationException($"User with email {dto.Email} already exists.");

    //    var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, BCrypt.Net.BCrypt.GenerateSalt());

    //    var user = new User
    //    {
    //        Id = Guid.NewGuid(),
    //        FullName = dto.FullName.Trim(),
    //        Email = dto.Email.Trim().ToLowerInvariant(),
    //        PasswordHash = passwordHash,
    //        Headline = string.Empty,
    //        Country = string.Empty,
    //        ProfilePhotoUrl = string.Empty,
    //        IsActive = true,
    //        Role = Role.Strategist,
    //        DateCreated = DateTime.UtcNow,
    //        IsDeleted = false
    //    };

    //    await _userRepository.AddAsync(user);
    //    await _userRepository.SaveChangesAsync();

    //    var accessToken = GenerateJwtToken(user);
    //    var refreshToken = await GenerateRefreshTokenAsync(user.Id, ipAddress);

    //    return new AuthResponseDto
    //    {
    //        AccessToken = accessToken,
    //        RefreshToken = refreshToken.Token,
    //        ExpiresIn = int.Parse(_configuration["JwtSettings:AccessTokenExpirationMinutes"] ?? "15") * 60,
    //        User = MapToUserDto(user)
    //    };
    //}

    public async Task<AuthResponseDto> RegisterIndividualAsync(RegisterIndividualRequest dto, string ipAddress)
    {
        var existingUser = await _userRepository.FirstOrDefaultAsync(u => u.Email == dto.Email.ToLowerInvariant());
        if (existingUser != null)
            throw new InvalidOperationException($"User with email {dto.Email} already exists.");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, BCrypt.Net.BCrypt.GenerateSalt());

        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            FullName = $"{dto.FirstName.Trim()} {dto.LastName.Trim()}",
            Email = dto.Email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            Certification = dto.Certification,
            Title = dto.Title,
            ShortBio = dto.ShortBio,
            CvFileUrl = dto.CvFileUrl ?? string.Empty,
            Headline = string.Empty,
            Country = dto.Country.Trim(),
            ProfilePhotoUrl = string.Empty,
            IsActive = true,
            Role = Role.Strategist,
            DateCreated = DateTime.UtcNow,
            IsDeleted = false
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        var accessToken = GenerateJwtToken(user);
        var refreshToken = await GenerateRefreshTokenAsync(user.Id, ipAddress);

        // ✅ CHANGED: Return consistent AuthResponseDto
        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresIn = int.Parse(_configuration["JwtSettings:AccessTokenExpirationMinutes"] ?? "15") * 60,
            User = MapToUserDto(user)
        };
    }

    public async Task<AuthResponseDto> RegisterCorporateAsync(RegisterCorporateRequest dto, string ipAddress)
    {
        var existingUser = await _userRepository.FirstOrDefaultAsync(u => u.Email == dto.RepresentativeEmail.ToLowerInvariant());
        if (existingUser != null)
            throw new InvalidOperationException($"Representative email {dto.RepresentativeEmail} already in use.");

        var corporate = new CorporateAccount
        {
            Id = Guid.NewGuid(),
            OrganisationName = dto.OrganisationName,
            RepresentativeFirstName = dto.RepresentativeFirstName,
            RepresentativeLastName = dto.RepresentativeLastName,
            RepresentativeEmail = dto.RepresentativeEmail.ToLowerInvariant(),
            PhoneNumber = dto.PhoneNumber,
            Country = dto.Country,
            Sector = dto.Sector,
            CompanyOverview = dto.CompanyOverview,
            ContributionInterestAreasJson = System.Text.Json.JsonSerializer.Serialize(dto.ContributionInterestAreas),
            SupportingDocumentsJson = System.Text.Json.JsonSerializer.Serialize(dto.SupportingDocuments),
            OptionalNotes = dto.OptionalNotes ?? string.Empty,
            DeclarationAccepted = dto.DeclarationAccepted,
            PaidMemberSlots = 0,
            UsedMemberSlots = 0,
            DateCreated = DateTime.UtcNow,
            IsDeleted = false
        };

        await _corporateAccountRepository.AddAsync(corporate);
        await _corporateAccountRepository.SaveChangesAsync();

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, BCrypt.Net.BCrypt.GenerateSalt());

        var representative = new User
        {
            Id = Guid.NewGuid(),
            FirstName = dto.RepresentativeFirstName,
            LastName = dto.RepresentativeLastName,
            FullName = $"{dto.RepresentativeFirstName} {dto.RepresentativeLastName}",
            Email = dto.RepresentativeEmail.ToLowerInvariant(),
            PasswordHash = passwordHash,
            Headline = string.Empty,
            Country = dto.Country,
            ProfilePhotoUrl = string.Empty,
            IsActive = true,
            Role = Role.CorporateAdmin,
            CorporateAccountId = corporate.Id,
            DateCreated = DateTime.UtcNow,
            IsDeleted = false
        };

        await _userRepository.AddAsync(representative);
        await _userRepository.SaveChangesAsync();

        corporate.UsedMemberSlots = 1;
        corporate.DateUpdated = DateTime.UtcNow;
        await _corporateAccountRepository.UpdateAsync(corporate);
        await _corporateAccountRepository.SaveChangesAsync();

        var accessToken = GenerateJwtToken(representative);
        var refreshToken = await GenerateRefreshTokenAsync(representative.Id, ipAddress);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresIn = int.Parse(_configuration["JwtSettings:AccessTokenExpirationMinutes"] ?? "15") * 60,
            User = MapToUserDto(representative)
        };
    }

    public async Task<CorporateInvite> InviteTeamMemberAsync(InviteTeamMemberRequest dto)
    {
        // Temporarily disabled: Payment restriction removed per director's request
        // await _corporateService.ValidateInviteCapacityAsync(dto.CorporateAccountId);

        var corporate = await _corporateAccountRepository.GetByIdAsync(dto.CorporateAccountId) ??
            throw new KeyNotFoundException("Corporate account not found.");

        var token = Guid.NewGuid().ToString();
        var invite = new CorporateInvite
        {
            Id = Guid.NewGuid(),
            CorporateAccountId = corporate.Id,
            Email = dto.Email.ToLowerInvariant(),
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsUsed = false,
            DateCreated = DateTime.UtcNow,
            IsDeleted = false
        };

        await _corporateInviteRepository.AddAsync(invite);
        await _corporateInviteRepository.SaveChangesAsync();

        await SendInviteEmailAsync(corporate, invite);

        return invite;
    }

    private async Task SendInviteEmailAsync(CorporateAccount corporate, CorporateInvite invite)
    {
        try
        {
            var frontendUrl = _configuration["AppSettings:FrontendUrl"] ?? "https://yourapp.com";
            var inviteLink = $"{frontendUrl}/register/team-member?token={invite.Token}&email={Uri.EscapeDataString(invite.Email)}";
            var expiryDate = invite.ExpiresAt.ToString("MMMM dd, yyyy");

            var subject = $"Invitation to Join {corporate.OrganisationName} on TBP Platform";

            var plainTextContent = $@"
                Hello,

                You have been invited to join {corporate.OrganisationName} on the TBP Platform as a team member.

                To accept this invitation and create your account, please click the link below:
                {inviteLink}

                This invitation will expire on {expiryDate}.

                If you did not expect this invitation, please ignore this email.

                Best regards,
                TBP Platform Team
                ";

                            var htmlContent = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
                        .content {{ background-color: #f9f9f9; padding: 30px; border-radius: 5px; margin-top: 20px; }}
                        .button {{ display: inline-block; padding: 12px 30px; background-color: #4CAF50; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
                        .footer {{ text-align: center; margin-top: 30px; font-size: 12px; color: #666; }}
                        .warning {{ background-color: #fff3cd; border-left: 4px solid #ffc107; padding: 10px; margin: 15px 0; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Team Invitation</h1>
                        </div>
                        <div class='content'>
                            <h2>Hello!</h2>
                            <p>You have been invited to join <strong>{corporate.OrganisationName}</strong> on the TBP Platform as a team member.</p>
            
                            <p>As part of the team, you will have access to:</p>
                            <ul>
                                <li>Collaborative strategic planning tools</li>
                                <li>Shared resources and reports</li>
                                <li>Team communication features</li>
                            </ul>

                            <p>To accept this invitation and create your account, please click the button below:</p>
            
                            <center>
                                <a href='{inviteLink}' class='button'>Accept Invitation</a>
                            </center>

                            <div class='warning'>
                                <strong>⏰ Important:</strong> This invitation will expire on <strong>{expiryDate}</strong>. Please register before this date.
                            </div>

                            <p>If the button doesn't work, copy and paste this link into your browser:</p>
                            <p style='word-break: break-all; color: #4CAF50;'>{inviteLink}</p>

                            <p>If you did not expect this invitation, please ignore this email.</p>
                        </div>
                        <div class='footer'>
                            <p>&copy; {DateTime.UtcNow.Year} TBP Platform. All rights reserved.</p>
                            <p>This is an automated email. Please do not reply.</p>
                        </div>
                    </div>
                </body>
                </html>
                ";

            var emailSent = await _emailService.SendEmailAsync(
                invite.Email,
                subject,
                plainTextContent,
                htmlContent
            );

            if (!emailSent)
            {
                var logger = _configuration.GetSection("Logging").Get<ILogger<AuthService>>();
                logger?.LogWarning("Failed to send invitation email to {Email} for corporate account {CorporateId}", 
                    invite.Email, corporate.Id);
            }
        }
        catch (Exception ex)
        {
            var logger = _configuration.GetSection("Logging").Get<ILogger<AuthService>>();
            logger?.LogError(ex, "Error sending invitation email to {Email} for corporate account {CorporateId}", 
                invite.Email, corporate.Id);
        }
    }

    public async Task<AuthResponseDto> RegisterTeamMemberAsync(RegisterTeamMemberRequest dto, string ipAddress)
    {
        var invite = await _corporateInviteRepository.Query()
            .Include(i => i.CorporateAccount)
            .FirstOrDefaultAsync(i => i.Token == dto.Token && i.Email == dto.Email.ToLowerInvariant());
            
        if (invite == null || invite.IsUsed || invite.ExpiresAt < DateTime.UtcNow)
            throw new InvalidOperationException("Invalid or expired invite token.");

        // Temporarily disabled: Payment restriction removed per director's request
        // await _corporateService.ValidateInviteCapacityAsync(invite.CorporateAccountId);

        var existingUser = await _userRepository.FirstOrDefaultAsync(u => u.Email == dto.Email.ToLowerInvariant());
        if (existingUser != null)
            throw new InvalidOperationException($"User with email {dto.Email} already exists.");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, BCrypt.Net.BCrypt.GenerateSalt());

        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            FullName = $"{dto.FirstName} {dto.LastName}",
            Email = dto.Email.ToLowerInvariant(),
            PasswordHash = passwordHash,
            Title = dto.Title,
            Country = dto.Country.Trim(),
            ProfilePhotoUrl = string.Empty,
            Certification = dto.Certification ?? string.Empty,
            ShortBio = dto.ShortBio ?? string.Empty,
            CvFileUrl = dto.CvFileUrl ?? string.Empty,
            IsActive = true,
            Role = Role.CorporateTeam,
            CorporateAccountId = invite.CorporateAccountId,
            DateCreated = DateTime.UtcNow,
            IsDeleted = false
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        var corporate = invite.CorporateAccount;
        corporate.UsedMemberSlots += 1;
        corporate.DateUpdated = DateTime.UtcNow;
        await _corporateAccountRepository.UpdateAsync(corporate);

        invite.IsUsed = true;
        invite.DateUpdated = DateTime.UtcNow;
        await _corporateInviteRepository.UpdateAsync(invite);
        await _corporateInviteRepository.SaveChangesAsync();

        var accessToken = GenerateJwtToken(user);
        var refreshToken = await GenerateRefreshTokenAsync(user.Id, ipAddress);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresIn = int.Parse(_configuration["JwtSettings:AccessTokenExpirationMinutes"] ?? "15") * 60,
            User = MapToUserDto(user)
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto, string ipAddress)
    {
        var user = await _userRepository.FirstOrDefaultAsync(u => u.Email == dto.Email.ToLowerInvariant());
        if (user == null)
            throw new UnauthorizedAccessException("Invalid email or password.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("User account is inactive.");

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        var accessToken = GenerateJwtToken(user);
        var refreshToken = await GenerateRefreshTokenAsync(user.Id, ipAddress);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresIn = int.Parse(_configuration["JwtSettings:AccessTokenExpirationMinutes"] ?? "15") * 60,
            User = MapToUserDto(user)
        };
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(string token, string ipAddress)
    {
        var refreshToken = await _refreshTokenRepository.Query()
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token);

        if (refreshToken == null)
            throw new UnauthorizedAccessException("Invalid refresh token.");

        if (!refreshToken.IsActive)
        {
            if (refreshToken.Revoked != null)
            {
                await RevokeAllUserTokensAsync(refreshToken.UserId, ipAddress, "Token reuse detected");
                throw new UnauthorizedAccessException("Token reuse detected. All tokens have been revoked.");
            }
            throw new UnauthorizedAccessException("Refresh token has expired.");
        }

        var user = refreshToken.User;
        if (user == null || !user.IsActive)
            throw new UnauthorizedAccessException("User account is inactive.");

        var newRefreshToken = await RotateRefreshTokenAsync(refreshToken, ipAddress);
        var accessToken = GenerateJwtToken(user);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken.Token,
            ExpiresIn = int.Parse(_configuration["JwtSettings:AccessTokenExpirationMinutes"] ?? "15") * 60,
            User = MapToUserDto(user)
        };
    }

    public async Task RevokeRefreshTokenAsync(string token, string ipAddress)
    {
        var refreshToken = await _refreshTokenRepository.Query()
            .FirstOrDefaultAsync(rt => rt.Token == token);

        if (refreshToken == null || !refreshToken.IsActive)
            return;

        refreshToken.Revoked = DateTime.UtcNow;
        refreshToken.RevokedByIp = ipAddress;
        refreshToken.DateUpdated = DateTime.UtcNow;

        await _refreshTokenRepository.UpdateAsync(refreshToken);
        await _refreshTokenRepository.SaveChangesAsync();
    }

    public async Task<UserDto?> GetCurrentUserAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        return user == null ? null : MapToUserDto(user);
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secret = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret not configured.");
        var issuer = jwtSettings["Issuer"] ?? "TBP";
        var audience = jwtSettings["Audience"] ?? "TBPUsers";
        var expirationMinutes = int.Parse(jwtSettings["AccessTokenExpirationMinutes"] ?? "15");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<RefreshToken> GenerateRefreshTokenAsync(Guid userId, string ipAddress)
    {
        var token = GenerateSecureRandomToken();
        var expirationDays = int.Parse(_configuration["JwtSettings:RefreshTokenExpirationDays"] ?? "30");

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = token,
            Expires = DateTime.UtcNow.AddDays(expirationDays),
            Created = DateTime.UtcNow,
            CreatedByIp = ipAddress,
            IsDeleted = false,
            DateCreated = DateTime.UtcNow
        };

        await _refreshTokenRepository.AddAsync(refreshToken);
        await _refreshTokenRepository.SaveChangesAsync();

        return refreshToken;
    }

    private async Task<RefreshToken> RotateRefreshTokenAsync(RefreshToken oldToken, string ipAddress)
    {
        oldToken.Revoked = DateTime.UtcNow;
        oldToken.RevokedByIp = ipAddress;
        oldToken.DateUpdated = DateTime.UtcNow;

        var newToken = GenerateSecureRandomToken();
        var expirationDays = int.Parse(_configuration["JwtSettings:RefreshTokenExpirationDays"] ?? "30");

        var newRefreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = oldToken.UserId,
            Token = newToken,
            Expires = DateTime.UtcNow.AddDays(expirationDays),
            Created = DateTime.UtcNow,
            CreatedByIp = ipAddress,
            ReplacedByToken = newToken,
            IsDeleted = false,
            DateCreated = DateTime.UtcNow
        };

        oldToken.ReplacedByToken = newToken;

        await _refreshTokenRepository.UpdateAsync(oldToken);
        await _refreshTokenRepository.AddAsync(newRefreshToken);
        await _refreshTokenRepository.SaveChangesAsync();

        return newRefreshToken;
    }

    private async Task RevokeAllUserTokensAsync(Guid userId, string ipAddress, string reason)
    {
        var tokens = await _refreshTokenRepository.Query()
            .Where(rt => rt.UserId == userId && rt.IsActive)
            .ToListAsync();

        foreach (var token in tokens)
        {
            token.Revoked = DateTime.UtcNow;
            token.RevokedByIp = ipAddress;
            token.DateUpdated = DateTime.UtcNow;
            await _refreshTokenRepository.UpdateAsync(token);
        }

        await _refreshTokenRepository.SaveChangesAsync();
    }

    private static string GenerateSecureRandomToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }

    private static UserDto MapToUserDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Headline = user.Headline,
            Country = user.Country,
            ProfilePhotoUrl = user.ProfilePhotoUrl,
            IsActive = user.IsActive,
            Role = user.Role,
            CorporateAccountId = user.CorporateAccountId,
            DateCreated = user.DateCreated,
            DateUpdated = user.DateUpdated
        };
    }
}

