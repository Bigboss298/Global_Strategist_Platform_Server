using Global_Strategist_Platform_Server.Model.Entities;

namespace Global_Strategist_Platform_Server.Model.Entities;

public class Report : BaseEntity
{
    public Guid StrategistId { get; set; }
    public Guid CategoryId { get; set; }
    public Guid ProjectId { get; set; }
    public Guid FieldId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public List<string> Attachments { get; set; } = [];
    public List<string> Links { get; set; } = [];
    
    // Navigation properties
    public User Strategist { get; set; } = null!;
    public Category Category { get; set; } = null!;
    public Project Project { get; set; } = null!;
    public Field Field { get; set; } = null!;
    public ICollection<Comment> Comments { get; set; } = [];
    public ICollection<Reaction> Reactions { get; set; } = [];
}

