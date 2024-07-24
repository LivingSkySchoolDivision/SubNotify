namespace SubNotify.Core;

public class SubRequest : IGUIDable
{
    public Guid Id { get; set; }
    public DateTime RequestedTimestamp { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Guid RequestorGUID { get;set; }
    public Guid SubGUID { get; set; }
    public Guid SchoolGUID { get; set; }
    public bool SubNeedsAccessToEmail { get; set; }
}
