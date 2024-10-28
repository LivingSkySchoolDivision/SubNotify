namespace SubNotify.Core;

public class AvailableSub : IGUIDable
{
    public Guid Id { get; set; }
    public bool IsEnabled { get; set; } = true;
    public string GivenName { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<Guid> SchoolGUIDs { get; set; } = new List<Guid>();

    public string DisplayName { 
        get 
        {
            if ((!string.IsNullOrEmpty(this.GivenName)) && (!string.IsNullOrEmpty(this.Surname)))
            {
                return this.Surname + ", " + this.GivenName;
            } else {
                return this.Id.ToString();
            }
        }
    }
}
