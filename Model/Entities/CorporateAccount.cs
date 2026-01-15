using Global_Strategist_Platform_Server.Model.Entities;

namespace Global_Strategist_Platform_Server.Model.Entities;

public class CorporateAccount : BaseEntity
{
    public const int FreeMemberSlots = 2;
    
    public string OrganisationName { get; set; } = string.Empty;
    public string RepresentativeFirstName { get; set; } = string.Empty;
    public string RepresentativeLastName { get; set; } = string.Empty;
    public string RepresentativeEmail { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Sector { get; set; } = string.Empty;
    public string CompanyOverview { get; set; } = string.Empty;
    public string ContributionInterestAreasJson { get; set; } = "[]"; // store as json
    public string SupportingDocumentsJson { get; set; } = "[]"; // store as json array of urls
    public string OptionalNotes { get; set; } = string.Empty;
    public bool DeclarationAccepted { get; set; }

    // Member capacity management
    public int PaidMemberSlots { get; set; } = 0;
    public int UsedMemberSlots { get; set; } = 0;

    // Computed properties
    public int TotalSlots => FreeMemberSlots + PaidMemberSlots;
    public int RemainingSlots => TotalSlots - UsedMemberSlots;

    // Navigation
    // Use Users collection to represent all users (representative + team members)
    public ICollection<User> Users { get; set; } = [];
    public ICollection<CorporateInvite> Invites { get; set; } = [];
    public ICollection<CorporatePayment> Payments { get; set; } = [];
}
