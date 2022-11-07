using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Raydreams.MicroCMS
{
    /// <summary>Get a binary image file only from the data store</summary>
    public class GetHomeFunction : BaseFunction
    {
        public GetHomeFunction( ICMSGateway gate ) : base( gate )
        { }

        [Function( "GetHome" )]
        public HttpResponseData Run( [HttpTrigger( AuthorizationLevel.Anonymous, "get", Route = "{var?}" )] HttpRequestData req, string var, FunctionContext ctx )
        {
            ILogger logger = ctx.GetLogger( "API" );
            logger.LogInformation( $"{GetType().Name} triggered." );

            return req.EmptyResponse();
        }
    }
}