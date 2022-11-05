using System;
using System.Linq;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Web;
using System.Net;
using Newtonsoft.Json;
using System.Text;
using System.Net.Mime;

namespace Raydreams.MicroCMS
{
	/// <summary>Extensions to HttpRequestData</summary>
	public static class RequestExtensions
	{
		/// <summary></summary>
		public const string StaleRequestKey = "x-timestamp";

		/// <summary></summary>
		public const string AuthHeaderKey = "x-authorization";

		/// <summary>Gets all the client headers we are interested in for later</summary>
		public static ClientHeaders GetClientHeaders( this HttpRequestData req )
		{
			if ( req == null || req.Headers == null )
				return new ClientHeaders();

			return new ClientHeaders
			{
				ClientIP = req.GetClientIP(),
				ClientAgent = req.GetHeaderValue( "User-Agent" ),
			};
		}

		/// <summary>Get a header value by its key</summary>
		/// <param name="key">Case sensitive key</param>
		public static string GetHeaderValue( this HttpRequestData req, string key )
		{
			if ( String.IsNullOrWhiteSpace( key ) )
				return null;

			key = key.Trim();

			if ( req.Headers.Contains( key ) )
				return req.Headers.GetValues( key ).FirstOrDefault()?.Trim();

			return null;
		}

		/// <summary>Specific to getting the IP address from the header without the client's port</summary>
		/// <param name="req"></param>
		/// <returns></returns>
		/// <remarks>Only tested with IPv4</remarks>
		public static string GetClientIP( this HttpRequestData req )
		{
			string value = req.GetHeaderValue( "X-Forwarded-For" );

			// should test for IP v4 vs v6
			if ( String.IsNullOrWhiteSpace( value ) )
				return null;

			int idx = value.LastIndexOf( ':' );
			if ( idx > 2 )
				return value.Substring( 0, idx );

			return value;
		}

        /// <summary></summary>
        /// <param name="key"></param>
        /// <param name="def">Default value to set to if the key DNE</param>
        /// <returns></returns>
        public static bool GetBoolValue( this HttpRequestData req, string key, bool def )
        {
            if ( String.IsNullOrWhiteSpace( key ) )
                return false;

            key = key.Trim();

            var query = HttpUtility.ParseQueryString( req.Url.Query );

            if ( !query.AllKeys.Contains( key ) )
                return def;

            return query[key].Trim().GetBooleanValue();
        }

        /// <summary>Validate a string value from the request</summary>
        /// <param name="key"></param>
        /// <param name="def">Default value to set to if the key DNE</param>
        /// <returns></returns>
        public static string GetStringValue( this HttpRequestData req, string key, string def = null )
		{
			if ( String.IsNullOrWhiteSpace( key ) )
				return def;

			key = key.Trim();

			var query = HttpUtility.ParseQueryString( req.Url.Query );

			if ( !query.AllKeys.Contains( key ) )
				return def;

			return query[key].Trim();
		}

		/// <summary></summary>
        /// <param name="req"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public static HttpResponseData BlobResponse( this HttpRequestData req, RawFileWrapper file )
        {
            // set the sream disposition
            ContentDisposition dispo = new ContentDisposition
            {
                FileName = file.Filename,
                Inline = true
            };

            HttpResponseData resp = req.CreateResponse(HttpStatusCode.OK );
            resp.Headers.Add( "Content-Type", file.ContentType );
			resp.Headers.Add( "Content-Disposition", dispo.ToString() );
			//resp.Headers.Add
            resp.WriteBytes( file.Data );
            return resp;
        }

        /// <summary>Returns an HTML response</summary>
        /// <param name="req"></param>
        /// <param name="results"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public static HttpResponseData HTMLResponse( this HttpRequestData req, APIResult<string> results, HttpStatusCode code = HttpStatusCode.OK )
        {
            HttpResponseData resp = req.CreateResponse( code );
            resp.Headers.Add( "Content-Type", "text/html; charset=utf-8" );
            resp.WriteString( results.ResultObject, Encoding.UTF8 );
            return resp;
        }

        /// <summary>Rolls a simple OK response with no body</summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public static HttpResponseData EmptyResponse( this HttpRequestData req )
		{
			HttpResponseData resp = req.CreateResponse( HttpStatusCode.OK );
			resp.Headers.Add( "Content-Type", "text/plain; charset=utf-8" );
			resp.Headers.Add( "Content-Length", "0" );
			return resp;
		}

        /// <summary>Return an OK Response using the Newton Soft JSON serializer</summary>
        /// <param name="req">Request Object</param>
        /// <param name="results">result data</param>
        /// <param name="code">Response code to use which will default to 200</param>
        /// <returns></returns>
        /// <remarks>Rename to JSON Response since it could be any kind of response</remarks>
        public static HttpResponseData OKResponse<T>( this HttpRequestData req, APIResult<T> results, HttpStatusCode code = HttpStatusCode.OK )
		{
			HttpResponseData resp = req.CreateResponse( code );
			resp.Headers.Add( "Content-Type", "text/json; charset=utf-8" );
			resp.WriteString( JsonConvert.SerializeObject( results ), Encoding.UTF8 );
			return resp;
		}

		/// <summary>Return a 400 Bad Request Response as plain text</summary>
		/// <param name="req"></param>
		/// <param name="errorMessage"></param>
		/// <returns></returns>
		public static HttpResponseData BadResponse( this HttpRequestData req, string errorMessage )
		{
			HttpResponseData resp = req.CreateResponse( HttpStatusCode.BadRequest );
			resp.Headers.Add( "Content-Type", "text/plain; charset=utf-8" );
			resp.WriteString( errorMessage, Encoding.UTF8 );

			return resp;
		}

		/// <summary></summary>
		/// <param name="req"></param>
		/// <param name="errorMessage"></param>
		/// <returns></returns>
		public static HttpResponseData Redirect( this HttpRequestData req, string url )
		{
			url = ( !String.IsNullOrWhiteSpace( url ) ) ? url.Trim() : String.Empty;

			HttpResponseData resp = req.CreateResponse( HttpStatusCode.Redirect );
			resp.Headers.Add( "Location", url );

			return resp;
		}

		/// <summary>Based on the environment will handle the correct error repsonse for debugging and logging</summary>
		/// <typeparam name="T">The normal return type of the function</typeparam>
		/// <param name="logger">The MS function logger</param>
		/// <param name="exp">exception</param>
		/// <param name="results">The results object that would have been returned to add debug to</param>
		/// <returns>In PROD returns a 500, otherwise a 200 with a Debug message</returns>
		public static HttpResponseData ReponseError<T>( this HttpRequestData req, APIResult<T> results, Exception exp, ILogger logger )
		{
			var type = Environment.GetEnvironmentVariable( "env" );
			EnvironmentType env = type.GetEnumValue<EnvironmentType>( EnvironmentType.Unknown, true );

			string error = exp.ToLogMsg( true );

			if ( env == EnvironmentType.Production )
			{
				logger.LogError( error, null );
				return req.CreateResponse( HttpStatusCode.InternalServerError );
			}

			results.Debug = error;

			HttpResponseData resp = req.CreateResponse( HttpStatusCode.OK );
			resp.Headers.Add( "Content-Type", "text/json; charset=utf-8" );
			resp.WriteString( JsonConvert.SerializeObject( results ) );

			return resp;
		}
	}
}
