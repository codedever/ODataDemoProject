using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace ODataDemoProject
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            var env = Environment.GetEnvironmentVariable(AppSettings.ASPNETCORE_ENVIRONMENT) ?? EnvironmentName.Production;
            var appsettings = AppSettings.APP_SETTINGS_FILE + AppSettings.DEFAULT_SEPARATOR + env + AppSettings.DEFAULT_SEPARATOR + AppSettings.APP_SETTINGS_FILE_EXTENSION;
            Console.WriteLine($"environment is: {env}, and appsettings file is: {appsettings}");
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(AppSettings.DEFAULT_APP_SETTINGS_FILE)
                .Build();

            Console.WriteLine($"hosts is: {config.GetValue<string>(AppSettings.HOSTS)}");
            return WebHost.CreateDefaultBuilder(args)
                .UseUrls(config.GetValue<string>(AppSettings.HOSTS).Split(AppSettings.DEFAULT_SPLITER, StringSplitOptions.RemoveEmptyEntries))
                .UseStartup<Startup>();
        }
    }
}
