using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Raydreams.MicroCMS
{
	/// <summary></summary>
	public partial class CMSGateway : ICMSGateway
	{
        #region [ Fields ]

        /// <summary>The logger to use will use null logger if non is set</summary>
        private ILogger _logger = null;

        /// <summary>Default environment</summary>
        private EnvironmentType _config = EnvironmentType.Unknown;

        /// <summary>Session timeout (secs) defaults to one hour</summary>
        private int _defaultTimeout = 3600 * 1;

        #endregion [ Fields ]

        #region [ Constructor ]

        /// <summary>Constructor</summary>
        /// <param name="env">The environment settings</param>
        /// <param name="auth">The Session Authorization Manager</param>
        /// <param name="domain">The domain this is being used on</param>
        public CMSGateway( EnvironmentSettings env )
        {
            // config and auth
            this.Config = env;

            // set the Markdown to HTML func
            this.ConvertMarkdown = MarkdownEngine.Markdown2HTML;

            // need to switch on the type of source
            // ICMSRepository
        }

        #endregion [ Constructor ]

        #region [ Properties ]

        /// <summary>Store the client agent</summary>
        protected string ClientAgent { get; set; } = "no client";

        /// <summary>Store the Client IP</summary>
        protected string ClientIP { get; set; } = "0.0.0.0";

        /// <summary>Client requested URL</summary>
        protected Uri RequestedURL { get; set; }

        /// <summary></summary>
        protected EnvironmentSettings Config { get; set; }

        /// <summary>The default logger</summary>
        public ILogger Logger { get; set; }

        /// <summary>Delegate Function for converting Markdown to HTML. Input string is markdown and return is HTML.</summary>
        /// <returns>Converted HTML</returns>
        /// <remarks>Supply your own Markdown conversion routine</remarks>
        public Func<string, string> ConvertMarkdown { get; set; }

        #endregion [ Properties ]

        /// <summary>Allows adding additional headers after the class is created</summary>
        /// <param name="req">The request that if null will set to default client header values</param>
        public ICMSGateway AddLogger( ILogger logger )
        {
            this.Logger = logger;

            return this;
        }

        /// <summary>Allows adding additional headers after the class is created</summary>
        /// <param name="req">The request that if null will set to default client header values</param>
        public ICMSGateway AddHeaders( HttpRequestData req )
        {
            ClientHeaders headers = req.GetClientHeaders();

            if ( headers == null )
                return this;

            this.ClientIP = headers.ClientIP;
            this.ClientAgent = headers.ClientAgent;
            this.RequestedURL = req.Url;

            return this;
        }

        /// <summary>Gets the version of THIS assembly</summary>
        /// <returns></returns>
        public static string GetVersion()
        {
            var name = Assembly.GetExecutingAssembly().GetName();
            var vers = name.Version;

            return vers.ToString();
        }

        /// <summary>Just returns a simple signature string for testing</summary>
        /// <returns></returns>
        public string Ping( string msg )
        {
            this.Logger.LogInformation( $"Pinged = {msg}" );

            // default values
            string version = GetVersion();

            // create the signature
            return $"Service : {this.GetType().FullName}; Version : {version}; Env : {this.Config.EnvironmentType}; Message : {msg}";
        }

        /// <summary></summary>
        /// <returns></returns>
        public string RedirectHome() => $"{this.RequestedURL.Scheme}://{this.RequestedURL.Host}:{this.RequestedURL.Port}/page/{this.Config.DefaultHome}";

        /// <summary></summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public string GetPage( string file, string template, bool wrapped = false )
        {
			// validate input
            file = String.IsNullOrWhiteSpace( file ) ? $"{this.Config.DefaultHome}.md" : $"{file.Trim()}.md";

            // use the default template if none is defined - needs to be in Config
            template = String.IsNullOrWhiteSpace( template ) ? $"main.{this.Config.LayoutExtension}" : $"{template.Trim()}.{this.Config.LayoutExtension}";

            this.Logger?.LogInformation( $"Request for page {file}" );

            // get the files
            AzureFileShareRepository repo = new AzureFileShareRepository( this.Config.FileStore );

            // BUG - need to test the file exists first - if not return 404
            PageDetails md = repo.GetTextFile( this.Config.BlobRoot, file );
            PageDetails layout = repo.GetTextFile(this.Config.BlobRoot, $"{this.Config.LayoutsDir}/{template}" );

            if ( String.IsNullOrWhiteSpace( md.Content ) )
				md.Content = "This is not the page you are looking for...";

            if ( wrapped )
                return md.Content;

			// convert to HTML
            string html = this.ConvertMarkdown( md.Content );

            // insert the body
            if ( String.IsNullOrWhiteSpace(layout.Content) )
                layout.Content = MarkdownEngine.SimpleHTML;

            layout.Content = layout.Content.Replace( @"{% BODY %}", html ).Replace( @"{% TIMESTAMP %}", md.LastUpdated.ToString("yyyy-MM-dd hh:mm:ss zzz") );

            return layout.Content;
        }

        /// <summary>Gets an image file from the images folder</summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public RawFileWrapper GetImage( string file )
        {
            // validate input - set a default image in Config
            file = String.IsNullOrWhiteSpace( file ) ? "PROS.jpeg" : file.Trim();

            // get the files
            AzureFileShareRepository repo = new AzureFileShareRepository( this.Config.FileStore );
            var image = repo.GetRawFile( this.Config.BlobRoot, $"{this.Config.ImagesDir}/{file}" );

            return image;
        }

        /// <summary>Gets a list of all the content pages</summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public string ListPages(string template)
        {
            template = String.IsNullOrWhiteSpace( template ) ? $"main.{this.Config.LayoutExtension}" : $"{template.Trim()}.{this.Config.LayoutExtension}";

            StringBuilder sb = new StringBuilder("<ul>");

            // get the files
            AzureFileShareRepository repo = new AzureFileShareRepository( this.Config.FileStore);
            List<string> pages = repo.ListFiles( this.Config.BlobRoot );
            pages.ForEach( (s) => {

                foreach ( string ex in this.Config.ExcludeFolders )
                {
                    if ( s.StartsWith( ex, StringComparison.InvariantCultureIgnoreCase ) )
                        return;
                }

                string page = s.TrimExtension();
                sb.AppendLine($"<li><a href=\"/page/{page}\">{page}</a></li>");
            } );
            sb.AppendLine($"</ul>");

            var layout = repo.GetTextFile( this.Config.BlobRoot, $"{this.Config.LayoutsDir}/{template}" );

            if ( !String.IsNullOrWhiteSpace(layout.Content) )
            {
                layout.Content = layout.Content.Replace( @"{% BODY %}", sb.ToString() ).Replace( @"{% TIMESTAMP %}", DateTimeOffset.Now.ToString( "yyyy-MM-dd hh:mm:ss zzz" ) );
                return layout.Content;
            }

            return sb.ToString();
        }
    }
}
