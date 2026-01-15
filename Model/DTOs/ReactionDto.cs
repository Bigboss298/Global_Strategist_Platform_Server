using Global_Strategist_Platform_Server.Model.Enum;

namespace Global_Strategist_Platform_Server.Model.DTOs;

public class ReactionDto
{
    public Guid Id { get; set; }
    public Guid ReportId { get; set; }
    public Guid StrategistId { get; set; }
    public string StrategistName { get; set; } = string.Empty;
    public ReactionType ReactionType { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateReactionDto
{
    public Guid ReportId { get; set; }
    public Guid StrategistId { get; set; }
    public ReactionType ReactionType { get; set; }
}

public class ReactionsSummaryDto
{
    public int Like { get; set; }
    public int Love { get; set; }
    public int Insightful { get; set; }
    public int Dislike { get; set; }
}

