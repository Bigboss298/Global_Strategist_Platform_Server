namespace Global_Strategist_Platform_Server.Model.DTOs;

public class FieldDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime DateCreated { get; set; }
    public DateTime? DateUpdated { get; set; }
}

public class CreateFieldDto
{
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class UpdateFieldDto
{
    public string Name { get; set; } = string.Empty;
    public Guid? ProjectId { get; set; }
}

