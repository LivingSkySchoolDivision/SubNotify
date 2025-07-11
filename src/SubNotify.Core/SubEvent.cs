namespace SubNotify.Core;

public class SubEvent : IGUIDable
{
    public Guid Id { get; set; }
    public DateTime RequestedTimestampUTC { get; set; }

    private DateTime _startDate;
    public DateTime StartDate
    {
        get => _startDate;
        set
        {
            this._startDate = value;
            if (this._startDate > this._endDate)
            {
                this._endDate = this._startDate;
            }
        }
    }

    private DateTime _endDate;
    public DateTime EndDate
    {
        get => _endDate;
        set
        {
            this._endDate = value;
            if (this._endDate < this._startDate)
            {
                this._startDate = this._endDate;
            }
        }
    }
    public string RequestorOID { get; set; }
    public string RequestorName { get; set; }
    public string RequestorEmail { get; set; }
    public string SubstituteFor { get; set; }
    public Guid SubGUID { get; set; }
    public string SubName { get; set; }
    public Guid SchoolGUID { get; set; }
    public string SchoolName { get; set; }
    public bool SubNeedsAccessToEmail { get; set; }
    public bool TicketCreated_Onboard { get; set; } = false;
    public bool TicketCreated_Offboard { get; set; } = false;
    public DateTime LastNotifyTimestamp { get; set; }
    public string Notes { get; set; }
    public bool IsCancelled { get; set; }

    public override string ToString()
    {

        string returnMe = @"
            { 
                Id: " + this.Id.ToString() + @" 
                IsCancelled: " + this.IsCancelled + @"
                RequestedTimestamp: " + this.RequestedTimestampUTC + @"
                Start: " + this.StartDate.ToLongDateString() + " " + this.StartDate.ToLongTimeString() + @"
                End: " + this.EndDate.ToLongDateString() + " " + this.EndDate.ToLongTimeString() + @"
                RequestorOID: " + this.RequestorOID.ToString() + @"
                RequestorName: " + this.RequestorName + @"
                RequestorEmail: " + this.RequestorEmail + @"
                SubGUID: " + this.SubGUID.ToString() + @"
                SubName: " + this.SubName + @"
                SchoolGUID: " + this.SchoolGUID.ToString() + @"
                SchoolName: " + this.SchoolName + @"
                NeedEmailAccess: " + this.SubNeedsAccessToEmail + @"
                TicketCreated_Onboard: " + this.TicketCreated_Onboard + @"
                TicketCreated_Offboard: " + this.TicketCreated_Offboard + @"
            }
            ";

        return returnMe;
    }
}