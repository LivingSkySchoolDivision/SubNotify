namespace SubNotify.Core;

public class JIRAAPIResult : IGUIDable
{
    public Guid Id { get; set; }
    public Guid SubEventID { get; set; }

    public DateTime TimestampUTC { get; set; }
    public string RequestJSON { get; set; }
    public string JSONResponse { get; set; }
    public bool Success { get; set; }
}
