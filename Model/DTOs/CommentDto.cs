namespace Global_Strategist_Platform_Server.Model.DTOs;

public class CommentDto
{
    public Guid Id { get; set; }
    public Guid ReportId { get; set; }
    public Guid StrategistId { get; set; }
    public string StrategistName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateCommentDto
{
    public Guid ReportId { get; set; }
    public Guid StrategistId { get; set; }
    public string Content { get; set; } = string.Empty;
}

public class UpdateCommentDto
{
    public string Content { get; set; } = string.Empty;
}

