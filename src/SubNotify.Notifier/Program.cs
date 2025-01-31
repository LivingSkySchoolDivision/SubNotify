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

        static async Task Main(string[] args)
        {   
            // Load configuration
            IConfiguration configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddUserSecrets<Program>()
                .Build();
            
            // Load time zone
            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(configuration["Settings:TimeZone"]);

            // Set up database connection
            string dbConnectionString = configuration.GetConnectionString("Internal") ?? string.Empty;
            MongoDbConnection mongoDatabase = new MongoDbConnection(dbConnectionString);            

            string jira_username = configuration["Settings:JiraUsername"] ?? string.Empty;
            string jira_api_key = configuration["Settings:JiraAPIKey"] ?? string.Empty;
            string jira_domain = configuration["Settings:JiraDomain"] ?? string.Empty;
            string jira_projectid = configuration["Settings:JiraProjectID"] ?? string.Empty;
            string jira_issue_type_id = configuration["Settings:JiraIssueTypeID"] ?? string.Empty;

            // Dump some settings to console so we know it's working
            Console.WriteLine($"Jira user: {jira_username}");
            Console.WriteLine($"Jira domain: {jira_domain}");
            Console.WriteLine($"Jira project: {jira_projectid}");
            Console.WriteLine($"Jira issue type: {jira_issue_type_id}");
            Console.WriteLine($"Timezone: {timeZone}");

            // Sanity checks

            if (string.IsNullOrEmpty(jira_username)) {
                Console.WriteLine("Missing jira_username");
                Environment.Exit(0);
            }

            if (string.IsNullOrEmpty(jira_domain)) {
                Console.WriteLine("Missing jira_domain");
                Environment.Exit(0);
            }

            if (string.IsNullOrEmpty(jira_projectid)) {
                Console.WriteLine("Missing jira_projectid");
                Environment.Exit(0);
            }

            if (string.IsNullOrEmpty(jira_issue_type_id)) {
                Console.WriteLine("Missing jira_issue_type_id");
                Environment.Exit(0);
            }

            if (string.IsNullOrEmpty(configuration["Settings:TimeZone"])) {
                Console.WriteLine("Missing timeZone");
                Environment.Exit(0);
            } 

            JiraAPI Jira = new JiraAPI(jira_username, jira_api_key, jira_domain, jira_projectid, jira_issue_type_id);

            while (true)
            {
                // Figure out what day it is in UTC, given the configured timezone.
                // It might not be the same day depending on what time it is.
                DateTime currentLocalTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);


                // Window to look for events
                DateTime startOfTodayLocalTime = new DateTime(currentLocalTime.Year, currentLocalTime.Month, currentLocalTime.Day, 0,0,0, DateTimeKind.Local);
                DateTime endOfTodayLocalTime = new DateTime(currentLocalTime.Year, currentLocalTime.Month, currentLocalTime.Day, 23, 59, 59, DateTimeKind.Local);
                DateTime startOfTodayConvertedToUTC = startOfTodayLocalTime.Add(timeZone.GetUtcOffset(currentLocalTime) * -1);
                DateTime endOfTodayConvertedToUTC = endOfTodayLocalTime.Add(timeZone.GetUtcOffset(currentLocalTime) * -1);
                
                // Window to notify
                DateTime startNotificationWindowLocal = new DateTime(currentLocalTime.Year, currentLocalTime.Month, currentLocalTime.Day, 5,0,0, DateTimeKind.Local);
                DateTime endNotificationWindowLocal = new DateTime(currentLocalTime.Year, currentLocalTime.Month, currentLocalTime.Day, 17, 0, 0, DateTimeKind.Local);
                DateTime startNotificationWindowUTC = startNotificationWindowLocal.Add(timeZone.GetUtcOffset(currentLocalTime) * -1);
                DateTime endNotificationWindowUTC = endNotificationWindowLocal.Add(timeZone.GetUtcOffset(currentLocalTime) * -1);

                Console.WriteLine("-----------------------------------");
                Console.WriteLine("Starting check for notifications...");
                Console.WriteLine($"Current UTC time is  (Should be offset): " + DateTime.UtcNow.ToLongDateString() + " " + DateTime.UtcNow.ToLongTimeString());
                Console.WriteLine($"Local timezone is: {timeZone}");
                Console.WriteLine("Current timezone offset from UTC: " + timeZone.BaseUtcOffset);
                
                             
                Console.WriteLine($"Current local time is: " + currentLocalTime.ToLongDateString() + " " + currentLocalTime.ToLongTimeString());                
                Console.WriteLine($"Start of local day is (Local time) (Should be midnight): " + startOfTodayLocalTime.ToLongDateString() + " " + startOfTodayLocalTime.ToLongTimeString());
                Console.WriteLine($"End of local day is (Local time) (Should be 1 minute before midnight): " + endOfTodayLocalTime.ToLongDateString() + " " + endOfTodayLocalTime.ToLongTimeString());

                Console.WriteLine($"Start of day converted to UTC (UTC) (Should be offset): " + startOfTodayConvertedToUTC.ToLongDateString() + " " + startOfTodayConvertedToUTC.ToLongTimeString());
                Console.WriteLine($"End of day converted to UTC (UTC) (Should be offset): " + endOfTodayConvertedToUTC.ToLongDateString() + " " + endOfTodayConvertedToUTC.ToLongTimeString());

                Console.WriteLine($"Start of Notification window is (Local): " + startNotificationWindowLocal.ToLongDateString() + " " + startNotificationWindowLocal.ToLongTimeString());
                Console.WriteLine($"End of Notification window is (Local): " + endNotificationWindowLocal.ToLongDateString() + " " + endNotificationWindowLocal.ToLongTimeString());

                Console.WriteLine($"Start of Notification window is (UTC) (Should be offset): " + startNotificationWindowUTC.ToLongDateString() + " " + startNotificationWindowUTC.ToLongTimeString());
                Console.WriteLine($"End of Notification window is (UTC) (Should be offset): " + endNotificationWindowUTC.ToLongDateString() + " " + endNotificationWindowUTC.ToLongTimeString());

                if (!(
                    (DateTime.UtcNow >= startNotificationWindowUTC) && 
                    (DateTime.UtcNow <= endNotificationWindowUTC)
                    ))
                {                    
                    // Sleep until the next check
                    Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd-HH:mm:ss")}: Outside notification window - sleeping for {_sleepMinutes} minutes...");
                    Task.Delay(_sleepMinutes * 60 * 1000).Wait();
                    continue;
                } 
                
                // Get any sub events that happen to fall between the two converted dates, that haven't been processed yet
                MongoRepository<SubEvent> subEventRepo = new MongoRepository<SubEvent>(mongoDatabase);
                List<SubEvent> subEvents = subEventRepo.Find(x => 
                    (x.IsCancelled != true) && 
                    (x.StartDate >= startOfTodayConvertedToUTC) &&
                    (x.EndDate <= endOfTodayConvertedToUTC) &&
                    (
                        (x.TicketCreated_Onboard == false) || 
                        (x.TicketCreated_Offboard == false)
                    )
                ).ToList<SubEvent>();

                Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd-HH:mm:ss")} Found {subEvents.Count} events to notify for...");
                if (subEvents.Count > 0) 
                {
                    // Create onboarding tickets
                    foreach(SubEvent e in subEvents.Where(x => x.TicketCreated_Onboard == false)) 
                    {
                        Console.Write($"{DateTime.Now.ToString("yyyy-MM-dd-HH:mm:ss")} > Creating onboarding ticket for request {e.Id}...");
                        e.TicketCreated_Onboard = await Jira.CreateOnboardingTicket(e);
                        e.LastNotifyTimestamp = DateTime.Now;                        
                        subEventRepo.Update(e);              
                        Console.WriteLine(e.TicketCreated_Onboard ? "SUCCESS" : "FAILURE");                        
                    }
                    Task.Delay(5000).Wait();

                    // Create offboarding tickets
                    foreach(SubEvent e in subEvents.Where(x => x.TicketCreated_Offboard == false)) 
                    {
                        Console.Write($"{DateTime.Now.ToString("yyyy-MM-dd-HH:mm:ss")} > Creating offboarding ticket for request {e.Id}...");
                        e.TicketCreated_Offboard = await Jira.CreateOffboardingTicket(e);
                        e.LastNotifyTimestamp = DateTime.Now;
                        subEventRepo.Update(e);
                        Console.WriteLine(e.TicketCreated_Onboard ? "SUCCESS" : "FAILURE");
                    }
                    Task.Delay(5000).Wait();                    
                }

                // Sleep until the next check
                Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd-HH:mm:ss")}: Sleeping for {_sleepMinutes} minutes...");
                Task.Delay(_sleepMinutes * 60 * 1000).Wait();


            }
        }
    }
}