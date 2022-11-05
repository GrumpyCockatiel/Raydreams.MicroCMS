using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Raydreams.MicroCMS.Integrate
{
    /// <summary>Bootstrap Class</summary>
    public static class Program
    {
        /// <summary>Main entry class</summary>
        /// <param name="args">any future command line args</param>
        /// <returns>exit value</returns>
        public static int Main( string[] args )
        {
            Console.WriteLine( "Starting..." );

            // get the environment var
            string env = Environment.GetEnvironmentVariable( "ASPNETCORE_ENVIRONMENT" ) ?? "Development";

            // inject all the input
            IHostBuilder builder = new HostBuilder()
            .ConfigureLogging( ( ctx, logging ) =>
            {
                logging.AddConfiguration( ctx.Configuration.GetSection( "Logging" ) );
                logging.ClearProviders();
                logging.AddDebug();
                logging.AddConsole();
            } )
            .ConfigureAppConfiguration( ( ctx, config ) =>
            {
                config.AddJsonFile( $"appsettings.json", false, true )
                .AddJsonFile( $"appsettings.{env}.json", true, true );
                config.AddEnvironmentVariables();

                if ( args != null )
                    config.AddCommandLine( args );
            } )
            .ConfigureServices( ( ctx, services ) =>
            {
                services.AddOptions();

                // get the app config file
                services.AddScoped<TestConfig>( p => {
                    return ctx.Configuration.GetSection( "TestConfig" ).Get<TestConfig>();
                } );

                // add the logger
                services.AddLogging();

                // add hosted service
                services.AddHostedService<Tester>();
            } );

            // run the host sync
            // using just Build gives the Worker you can pass a cancellation token to
            builder.Build().Start();

            Console.WriteLine( "Stopping..." );

            return 0;
        }
    }


}