using System;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Raydreams.MicroCMS
{
    /// <summary>The API Gateway Interface</summary>
    public interface ICMSGateway
    {
        /// <summary></summary>
        ICMSGateway AddHeaders( HttpRequestData req );

        /// <summary></summary>
        ICMSGateway AddLogger( ILogger logger );

        /// <summary>Just Ping the API</summary>
        string Ping( string msg );

        /// <summary>Redirect back to the home page</summary>
        string RedirectHome();

        /// <summary>Get a page</summary>
        string GetPage( string file, string template, bool wrapped = false );

        /// <summary>Get an image</summary>
        RawFileWrapper GetImage( string file );

        /// <summary>List all the pages in non-excluded folders</summary>
        string ListPages( string template );
    }
}

