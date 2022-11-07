using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Raydreams.MicroCMS
{
    /// <summary>Get page intercepts requets to the root but will only return pages for now - no images.</summary>
    public class GetPageFunction : BaseFunction
    {
        public GetPageFunction( ICMSGateway gate ) : base( gate )
        { }

        [Function( "GetPage" )]
        public HttpResponseData Run( [HttpTrigger( AuthorizationLevel.Anonymous, "get", Route = "page/{file?}" )] HttpRequestData req, string file, FunctionContext ctx )
        {
            //$"{req.Url.Scheme}://{req.Url.Host}/page"

            ILogger logger = ctx.GetLogger( "API" );
            logger.LogInformation( $"{GetType().Name} triggered." );

            string layout = req.GetStringValue( "layout" );
            bool wrapped = req.GetBoolValue( "wrapped", false );

            // set the results
            APIResult<string> results = new APIResult<string>() { ResultCode = APIResultType.Unknown };

            try
            {
                this.Gateway.AddHeaders( req ).AddLogger(logger);

                file = ( String.IsNullOrWhiteSpace( file ) ) ? String.Empty : file.Trim();

                //if ( file.Equals( "list", StringComparison.InvariantCultureIgnoreCase ) )
                    //results.ResultObject = this.Gateway.ListPages( layout );
                //else // no verb then look for a page
                //{
                    //if ( file.StartsWith( "page", StringComparison.InvariantCultureIgnoreCase ) )
                        //file = file.Substring( 5 );
                    results.ResultObject = this.Gateway.GetPage( file, layout, wrapped );
                //}
            }
            catch ( Exception exp )
            {
                return req.ReponseError( results, exp, logger );
            }

            if ( results.ResultObject != null )
                results.ResultCode = APIResultType.Success;

            if ( !wrapped )
                return req.HTMLResponse( results );

            return req.OKResponse( results, System.Net.HttpStatusCode.OK );
        }
    }

}

