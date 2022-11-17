using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Raydreams.MicroCMS.IO;

namespace Raydreams.MicroCMS.CLI
{
    public class Program
    {
        /// <summary>Main entry class</summary>
        /// <param name="args">any future command line args</param>
        /// <returns>exit value</returns>
        public static int Main(string[] args)
        {
            Console.WriteLine("Starting...");

            // get the environment var
            string env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

            // inject all the input
            IHostBuilder builder = new HostBuilder()
            .ConfigureLogging((ctx, logging) =>
            {
                logging.AddConfiguration(ctx.Configuration.GetSection("Logging"));
                logging.ClearProviders();
                logging.AddDebug();
                logging.AddConsole();
            })
            .ConfigureAppConfiguration((ctx, config) =>
            {
                config.AddJsonFile($"appsettings.json", false, true)
                .AddJsonFile($"appsettings.{env}.json", true, true);
                config.AddEnvironmentVariables();

                if (args != null)
                    config.AddCommandLine(args);
            })
            .ConfigureServices((ctx, services) =>
            {
                services.AddOptions();

                AppConfig config = ctx.Configuration.GetSection("AppConfig").Get<AppConfig>();

                // get the app config file
                services.AddScoped<AppConfig>(p => { return config; } );

                // add the re-writer
                services.AddScoped<ICMSRepository>( p => new AzureFileShareRepository( config.ConnectionString ) );

                // add the logger
                services.AddLogging();

                // add hosted service
                services.AddHostedService<Watcher>();
            });

            // run the host sync
            // using just Build gives the Worker you can pass a cancellation token to
            builder.Build().Start();

            Console.WriteLine("Stopping...");

            return 0;
        }
    }
}