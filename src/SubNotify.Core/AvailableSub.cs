namespace SubNotify.Core;

public class AvailableSub : IGUIDable
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get ;set ; } = string.Empty;
    public List<Guid> SchoolGUIDs { get; set; } = new List<Guid>();
}
