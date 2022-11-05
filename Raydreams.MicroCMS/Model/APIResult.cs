using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Raydreams.MicroCMS
{
    /// <summary>This class is used to encapsulate the result of an API method call.</summary>
    public class APIResult<T>
    {
        #region [ Constructors ]

        public APIResult( T data, APIResultType code )
        {
            this.ResultCode = code;
            this.ResultObject = data;
        }

        public APIResult( APIResultType code ) : this( default, code )
        {
        }

        public APIResult() : this( default, APIResultType.Unknown )
        {
        }

        #endregion [ Constructors ]

        /// <summary>The error code on error</summary>
        [JsonProperty( "resultType" )]
        [JsonConverter( typeof( StringEnumConverter ) )]
        public APIResultType ResultCode { get; set; }

        /// <summary>The resulting data of a successful API method call</summary>
        [JsonProperty( "result" )]
        public T ResultObject { get; set; }

        /// <summary>Returns whether or not the API method call was successful.</summary>
        [JsonProperty( "isSuccess" )]
        public bool IsSuccess => this.ResultCode == APIResultType.Success;

        /// <summary>Any additional debugging info</summary>
        /// <remarks>In a Prod env needs to be set to JsonIgnore</remarks>
        [JsonProperty( "debug" )]
        public string Debug { get; set; }
    }
}

