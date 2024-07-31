namespace SubNotify.Core;

public class AvailableSub : IGUIDable
{
    public Guid Id { get; set; }
    public bool IsEnabled { get; set; } = true;
    public string GivenName { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string Email { get ;set ; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<Guid> SchoolGUIDs { get; set; } = new List<Guid>();
}
