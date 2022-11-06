using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Raydreams.MicroCMS
{
    /// <summary></summary>
    public class GetPageFunction : BaseFunction
    {
        public GetPageFunction( ICMSGateway gate ) : base( gate )
        { }

        [Function( "GetPage" )]
        public HttpResponseData Run( [HttpTrigger( AuthorizationLevel.Anonymous, "get", Route = "page/{file?}" )] HttpRequestData req, string file, FunctionContext ctx )
        {
            ILogger logger = ctx.GetLogger( "API" );
            logger.LogInformation( $"{GetType().Name} triggered." );

            string layout = req.GetStringValue( "layout" );
            bool wrapped = req.GetBoolValue( "wrapped", false );

            // set the results
            APIResult<string> results = new APIResult<string>() { ResultCode = APIResultType.Unknown };

            try
            {
                this.Gateway.AddHeaders( req ).AddLogger(logger);
                results.ResultObject = this.Gateway.GetPage( file?.Trim(), layout, wrapped );
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

