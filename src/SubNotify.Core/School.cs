namespace SubNotify.Core;

public class School : IGUIDable
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public bool IsEnabled { get; set; } = true;

    public override string ToString()
    {
        return "{ School Name='" + this.Name + "' Enabled='"+this.IsEnabled+"'} ";
    }
}
