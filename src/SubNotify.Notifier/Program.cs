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
                // Load any sub events that have come in that hvae notify set to false
                MongoRepository<SubEvent> subEventRepo = new MongoRepository<SubEvent>(mongoDatabase);

                // Load any events that need an onboarding ticket created
                // Load any events that need an offboarding ticket created
                List<SubEvent> subEvents = subEventRepo.Find(x => ((x.IsCancelled != true) && (x.TicketCreated_Onboard == false) || (x.TicketCreated_Offboard == false))).ToList<SubEvent>();

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

                // Sleep
                Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd-HH:mm:ss")}: Sleeping for {_sleepMinutes} minutes...");
                Task.Delay(_sleepMinutes * 60 * 1000).Wait();
            }
        }
    }
}