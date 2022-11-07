using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

namespace Raydreams.MicroCMS
{
	public class Program
	{
		public static void Main()
		{
			// get Config vars
            var env = EnvironmentSettings.GetSettings( Environment.GetEnvironmentVariable(EnvironmentSettings.EnvironmentKey) );

            IHost host = new HostBuilder()
				.ConfigureFunctionsWorkerDefaults()
				.ConfigureLogging( log => {
                    log.ClearProviders();
					log.AddProvider( new AzureTableLoggerProvider( env.FileStore ) );
				} )
				.ConfigureServices( (ctx, s) => {
					s.AddScoped<ICMSGateway>( p => {
                        var env = EnvironmentSettings.GetSettings(Environment.GetEnvironmentVariable(EnvironmentSettings.EnvironmentKey));
                        ICMSGateway gate = new CMSGateway( env );
						return gate;
					} );
				} )
				.Build();

			host.Run();
		}
	}
}