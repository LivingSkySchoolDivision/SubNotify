namespace SubNotify.Core;

public class School : IGUIDable
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
