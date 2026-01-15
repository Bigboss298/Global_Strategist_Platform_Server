using Global_Strategist_Platform_Server.Model.Entities;
using Global_Strategist_Platform_Server.Model.Enum;

namespace Global_Strategist_Platform_Server.Model.Entities;

public class Reaction : BaseEntity
{
    public Guid ReportId { get; set; }
    public Guid StrategistId { get; set; }
    public ReactionType ReactionType { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public Report Report { get; set; } = null!;
    public User Strategist { get; set; } = null!;
}

