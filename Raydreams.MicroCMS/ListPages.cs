using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Raydreams.MicroCMS
{
    public class ListPagesFunction : BaseFunction
    {
        private readonly ILogger _logger;

        public ListPagesFunction( ICMSGateway gate ) : base( gate )
        { }

        [Function( "ListPages" )]
        public HttpResponseData Run( [HttpTrigger( AuthorizationLevel.Anonymous, "get", Route = "list" )] HttpRequestData req, FunctionContext ctx )
        {
            ILogger logger = ctx.GetLogger( "API" );
            logger.LogInformation( $"{GetType().Name} triggered." );

            string layout = req.GetStringValue( "layout" );

            // set the results
            APIResult<string> results = new APIResult<string>() { ResultCode = APIResultType.Unknown };

            try
            {
                this.Gateway.AddHeaders( req ).AddLogger( logger );
                results.ResultObject = Gateway.ListPages( layout );
            }
            catch ( Exception exp )
            {
                return req.ReponseError( results, exp, logger );
            }

            if ( results.ResultObject != null )
                results.ResultCode = APIResultType.Success;

            return req.HTMLResponse( results );
        }
    }
}
