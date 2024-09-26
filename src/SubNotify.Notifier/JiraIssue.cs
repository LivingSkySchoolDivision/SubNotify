namespace SubNotify.Notifier
{
    public class JiraIssue 
    {
        public string Key { get; set; }
        public JiraIssueFields Fields { get; set; }
        //public DateTime DueDate { get; set; }
        //public string Summary { get; set; } = string.Empty;
        //public string Description { get; set; } = string.Empty;        
    }

    public class JiraIssueFields 
    {
        public string Summary { get; set; }
        //public DateTime DueDate { get; set; }
        //public string Summary { get; set; } = string.Empty;
        //public string Description { get; set; } = string.Empty;        
    }
}