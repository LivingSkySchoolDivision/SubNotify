using System;
using System.Net;
using System.Net.Mail;
using System.Text;
using LSSD.MongoDB;
using Microsoft.Extensions.Configuration;
using SubNotify.Core;

namespace SubNotify.Notifier
{
    internal class Program
    {
        private const int _sleepMinutes = 15;

        private static void ConsoleWrite(string message)
        {
            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm K") + ": " + message);
        }

        static void Main(string[] args)
        {   
            // Load configuration
            IConfiguration configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddUserSecrets<Program>()
                .Build();
            
            // Do sanity checks on the SMTP data from config
            
        
            // Load time zone
            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(configuration["Settings:TimeZone"]);
            ConsoleWrite($"Time zone: {timeZone}");

            // Set up database connection
            string dbConnectionString = configuration.GetConnectionString("Internal") ?? string.Empty;
            MongoDbConnection mongoDatabase = new MongoDbConnection(dbConnectionString);
        
            IConfigurationSection smtpConfig = configuration.GetSection("SMTP");
            SMTPConnectionDetails smtpInfo = new SMTPConnectionDetails()
            {
                Host = smtpConfig["hostname"],
                Port = int.Parse(smtpConfig["port"]),
                Username = smtpConfig["username"],
                Password = smtpConfig["password"],
                ReplyToAddress = smtpConfig["replytoaddress"],
                To = smtpConfig["to"],
                FromAddress = smtpConfig["fromaddress"]
            };            

            while (true)
            {
                // Load any sub events that have come in that hvae notify set to false
                MongoRepository<SubEvent> subEventRepo = new MongoRepository<SubEvent>(mongoDatabase);

                List<SubEvent> subEvents = subEventRepo.Find(x => x.NotificationSent == false).ToList<SubEvent>();


                if (subEvents.Count > 0) 
                {
                    using (SmtpClient smtpClient = new SmtpClient(smtpInfo.Host)
                    {
                        Port = smtpInfo.Port,
                        UseDefaultCredentials = false,
                        EnableSsl = true,
                        Credentials = new NetworkCredential(smtpInfo.Username, smtpInfo.Password)
                    })
                    {
                        foreach(SubEvent e in subEvents)
                        {
                            Console.WriteLine($"Sending notification for Id {e.Id.ToString()}");

                            MailMessage msg = new MailMessage();                
                            msg.To.Add(smtpInfo.To);
                            msg.Body = GenerateEmailBody(e, timeZone);
                            msg.Subject = GenerateEmailSubject(e);
                            msg.From = new MailAddress(smtpInfo.FromAddress, "SubNotify");
                            msg.ReplyToList.Add(new MailAddress(smtpInfo.ReplyToAddress, "SubNotify"));
                            msg.IsBodyHtml = true;                         
                            smtpClient.Send(msg);
                        }
                    }
                }

                // Sleep
                ConsoleWrite($"Sleeping for {_sleepMinutes} minutes...");
                Task.Delay(_sleepMinutes * 60 * 1000).Wait();
            }
        }

        private static string GenerateEmailSubject(SubEvent subEvent)
        {
            return $"Substitute Secretary - {subEvent.StartDate.ToShortDateString()} to {subEvent.EndDate.ToShortDateString()} - {subEvent.SchoolName}";
        }

        private static string GenerateEmailBody(SubEvent subEvent, TimeZoneInfo timezone) 
        {

            StringBuilder msgBody = new StringBuilder();

            msgBody.Append("<html>");
            msgBody.Append("<body>");
            msgBody.Append("<h3>Substitute Secretary Notification</h3>");
            msgBody.Append("<p>A substitute secretary notification has been submitted - see details below.</p>");
            msgBody.Append("<table>");

            msgBody.Append($"<tr><td><b>Submitted timestamp:</b></td><td>{TimeZoneInfo.ConvertTimeFromUtc(subEvent.RequestedTimestampUTC, timezone).ToLongDateString()} {subEvent.RequestedTimestampUTC.ToLongTimeString()}</td></tr>");
            msgBody.Append($"<tr><td><b>School:</b></td><td>{subEvent.SchoolName}</td></tr>");
            msgBody.Append($"<tr><td><b>Submitted by:</b></td><td>{subEvent.RequestorName} ({subEvent.RequestorEmail})</td></tr>");
            
            msgBody.Append($"<tr><td><b>Start Date:</b></td><td>{subEvent.StartDate.ToLongDateString()}</td></tr>");
            msgBody.Append($"<tr><td><b>End Date:</b></td><td>{subEvent.EndDate.ToLongDateString()}</td></tr>");
            msgBody.Append($"<tr><td><b>Substitute:</b></td><td>{subEvent.SubName}</td></tr>");
            msgBody.Append($"<tr><td><b>Needs access to email?:</b></td><td>{(subEvent.SubNeedsAccessToEmail ? "✅ YES" : "⛔ no")}</td></tr>");
            msgBody.Append($"<tr><td><b>Request ID:</b></td><td>{subEvent.Id.ToString()}</td></tr>");

            msgBody.Append("</table>");
            msgBody.Append("<p>To enable Teams notifications for the ticket that this email creates, set the \"Due Date\" field in Jira to one (1) work day (or more) before the \"Start Date\" listed above. This will trigger an alert to Teams every day at 3:00pm for any ticket due in the next day.</p>");            
            msgBody.Append("<p>The \"Request ID\" can be used to find this record in the SubNotify internal database, if troubleshooting is required.</p>");
            msgBody.Append("<p>This email was sent from an automated script, using an unmonitored mailbox. If you require further information, contact the requestor or school listed above.</p>");
            msgBody.Append("</body>");
            msgBody.Append("</html>");

            return msgBody.ToString();
            
        }
    }
}