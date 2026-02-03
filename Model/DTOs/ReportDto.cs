using Global_Strategist_Platform_Server.Model.Enum;

namespace Global_Strategist_Platform_Server.Model.DTOs;

public class ReportDto
{
    public Guid Id { get; set; }
    public Guid StrategistId { get; set; }
    public Guid CategoryId { get; set; }
    public Guid ProjectId { get; set; }
    public Guid FieldId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public List<string> Attachments { get; set; } = new List<string>();
    public List<string> Links { get; set; } = new List<string>();
    public DateTime DateCreated { get; set; }
    public DateTime? DateUpdated { get; set; }
}

public class ReportFeedDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public Guid StrategistId { get; set; }
    public string StrategistName { get; set; } = string.Empty;
    public string StrategistFirstName { get; set; } = string.Empty;
    public string StrategistLastName { get; set; } = string.Empty;
    public string StrategistCountry { get; set; } = string.Empty;
    public string StrategistProfilePhotoUrl { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string FieldName { get; set; } = string.Empty;
    public string ProjectImageUrl { get; set; } = string.Empty;
    public List<string> Images { get; set; } = new List<string>();
    public int CommentsCount { get; set; }
    public ReactionsSummaryDto ReactionsSummary { get; set; } = new ReactionsSummaryDto();
    public ReactionType? UserReaction { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime? DateUpdated { get; set; }
    public BadgeType BadgeType { get; set; }

}

public class CreateReportDto
{
    public Guid StrategistId { get; set; }
    public Guid CategoryId { get; set; }
    public Guid ProjectId { get; set; }
    public Guid FieldId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public List<string> Attachments { get; set; } = new List<string>();
    public List<string> Links { get; set; } = new List<string>();
}

public class UpdateReportDto
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public List<string> Attachments { get; set; } = new List<string>();
    public List<string> Links { get; set; } = new List<string>();
}

