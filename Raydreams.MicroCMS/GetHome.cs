using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Raydreams.MicroCMS
{
    /// <summary>For now intercepts rquest to the top level and sends them to the home page</summary>
    /// <remarks>Could be modified to get other types of files or to remove page completely</remarks>
    public class GetHomeFunction : BaseFunction
    {
        public GetHomeFunction( ICMSGateway gate ) : base( gate )
        { }

        [Function( "GetHome" )]
        public HttpResponseData Run( [HttpTrigger( AuthorizationLevel.Anonymous, "get", Route = "" )] HttpRequestData req, string file, FunctionContext ctx )
        {
            ILogger logger = ctx.GetLogger( "API" );
            logger.LogInformation( $"{GetType().Name} triggered." );

            return req.Redirect( $"{req.Url.Scheme}://{req.Url.Host}/page" );
        }
    }
}

