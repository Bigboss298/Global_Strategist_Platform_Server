namespace Global_Strategist_Platform_Server.Model.Entities;

public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime? DateUpdated { get; set; }
    public bool IsDeleted { get; set; }
}

