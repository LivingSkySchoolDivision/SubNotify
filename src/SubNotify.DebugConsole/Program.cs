using System;
using System.Configuration;
using Microsoft.Extensions.Configuration;
using SubNotify.Core;
using LSSD.MongoDB;

namespace SubNotify.DebugConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Loading configuration...");
            IConfiguration config = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddUserSecrets<Program>()
                .Build();

            Console.WriteLine("Connecting to database...");
            MongoDbConnection connection = new MongoDbConnection(config.GetConnectionString("Internal"));

            MongoRepository<School> schoolRepo = new MongoRepository<School>(connection);

            List<School> schools = schoolRepo.GetAll().ToList();

            

            Console.WriteLine("Listing schools:");
            foreach (School school in schools)
            {
                Console.WriteLine($"School: {school.Name} {school.Id}");
            }

        }
    }
}