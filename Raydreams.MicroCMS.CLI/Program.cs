using System.Reflection;
using CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Raydreams.MicroCMS.IO;
using Raydreams.MicroCMS.Logging;

namespace Raydreams.MicroCMS.CLI
{
    public class Program
    {
        /// <summary>Main entry class</summary>
        /// <param name="args">any future command line args</param>
        /// <returns>exit value</returns>
        public static int Main(string[] args)
        {
            Console.WriteLine("Starting MicroCMS CLI ...");

            // get the environment var
            string env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

            // load all the CL args into BaseOptions first and cast them later
            //var types = LoadVerbs();
            CLIBaseOptions? options = new CLIBaseOptions();

            // populate the options
            _ = Parser.Default.ParseArguments<CLIBaseOptions>(args)
                .WithParsed<CLIBaseOptions>((o) => { options = o as CLIBaseOptions; })
                .WithNotParsed((e) => { Console.WriteLine("Failed to load arguments");Environment.Exit(-1); });

            // inject all the input
            IHostBuilder builder = new HostBuilder()
            .ConfigureLogging( (ctx, logging) =>
            {
                //logging.AddConfiguration( ctx.Configuration.GetSection("Logging") );
                logging.ClearProviders();
                //logging.AddDebug();
                logging.AddConsole();
                logging.AddProvider( new FileLogProvider(options.WatchRoot) );
            })
            .ConfigureAppConfiguration((ctx, config) =>
            {
                config.AddJsonFile($"appsettings.json", false, true)
                .AddJsonFile($"appsettings.{env}.json", true, true);
                config.AddEnvironmentVariables();

                if ( args != null )
                    config.AddCommandLine(args);
            })
            .ConfigureServices((ctx, services) =>
            {
                //var x = ctx.Configuration;

                services.AddOptions();

                AppConfig config = ctx.Configuration.GetSection("AppConfig").Get<AppConfig>();

                // set the local root from the CL
                if (options != null && !String.IsNullOrWhiteSpace(options.WatchRoot))
                    config.LocalRoot = options.WatchRoot;

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
            IHost host = builder.Build();
            host.Run();

            Console.WriteLine( "Stopping MicroCMS CLI ..." );

            return 0;
        }

        /// <summary>Once the CLI needs verbs this will load all the classes with VerbAttribute</summary>
        /// <returns></returns>
        private static Type[] LoadVerbs()
        {
            return Assembly.GetExecutingAssembly().GetTypes().Where(t => t.GetCustomAttribute<VerbAttribute>() != null).ToArray();
        }
    }
}