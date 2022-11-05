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
    public class GetImageFunction : BaseFunction
    {
        public GetImageFunction( ICMSGateway gate ) : base( gate )
        { }

        [Function( "GetImage" )]
        public HttpResponseData Run( [HttpTrigger( AuthorizationLevel.Anonymous, "get", Route = "image/{file?}" )] HttpRequestData req, string file, FunctionContext ctx )
        {
            ILogger logger = ctx.GetLogger( "API" );
            logger.LogInformation( $"{GetType().Name} triggered." );

            string layout = req.GetStringValue( "layout" );

            // set the results
            RawFileWrapper results = new RawFileWrapper();

            try
            {
                this.Gateway.AddHeaders( req ).AddLogger( logger );
                results = Gateway.GetImage( file?.Trim() );
            }
            catch ( Exception exp )
            {
                return req.ReponseError( new APIResult<bool>() { ResultObject = false }, exp, logger );
            }

            return req.BlobResponse( results );
        }
    }
}

