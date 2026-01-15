using Global_Strategist_Platform_Server.Model.Entities;

namespace Global_Strategist_Platform_Server.Model.Entities;

public class Field : BaseEntity
{
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    
    // Navigation properties
    public Project Project { get; set; } = null!;
    public ICollection<Report> Reports { get; set; } = [];
}

