using Global_Strategist_Platform_Server.Model.Entities;

namespace Global_Strategist_Platform_Server.Model.Entities;

public class Comment : BaseEntity
{
    public Guid ReportId { get; set; }
    public Guid StrategistId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public Report Report { get; set; } = null!;
    public User Strategist { get; set; } = null!;
}

