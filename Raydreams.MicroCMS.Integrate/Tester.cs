using System;
using Microsoft.Extensions.Hosting;

namespace Raydreams.MicroCMS.Integrate
{
    public class Tester : BackgroundService
    {
        public Tester(TestConfig settings)
        {
            this.Settings = settings;
            this.Repo = new AzureFileShareRepository( this.Settings.ConnectionString );

            var config = EnvironmentSettings.DEV;
            config.BlobRoot = this.Settings.Root;
            config.FileStore = this.Settings.ConnectionString;
            this.Gateway = new CMSGateway( config );
        }

        public TestConfig Settings { get; set; }

        public AzureFileShareRepository Repo { get; set; }

        public ICMSGateway Gateway { get; set; }

        /// <summary></summary>
        protected override Task<int> ExecuteAsync( CancellationToken stoppingToken )
        {
            int res = 0;

            // lets do the thing
            try
            {
                res = this.Run();
            }
            catch ( System.Exception exp )
            {
                //this.LogException( exp );
                return Task.FromResult( -1 );
            }

            return Task.FromResult( res );
        }

        public int Run()
        {
            this.GetTextFileTest();

            this.ListFilesTest();

            return 0;
        }

        public bool GetTextFileTest()
        {
            var file = this.Repo.GetTextFile( "blog", "fresheyes.md" );

            return true;
        }

        public bool GetRawFileTest()
        {
            var file = this.Repo.GetRawFile( "blog", "images/PROS.jpeg" );

            return true;

        }
        public bool ListFilesTest()
        {
            List<string> file = this.Repo.ListFiles( "blog" );

            var list = this.Gateway.ListPages("main.txt");

            return true;

        }

    }

}

