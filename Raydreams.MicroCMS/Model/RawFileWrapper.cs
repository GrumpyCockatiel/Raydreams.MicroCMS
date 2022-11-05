using System;
using Newtonsoft.Json;

namespace Raydreams.MicroCMS
{
    /// <summary>Wraps raw bytes with some additional metadata</summary>
    /// <remarks>The data is stored as a byte array
    /// We can combine with the JSONFileWrapper by just making a custom serialier for the byte[]
    /// </remarks>
    public class RawFileWrapper
    {
        /// <summary>Original filename which should include the extension</summary>
        public string Filename { get; set; } = String.Empty;

        /// <summary>The MIME Type of the file.</summary>
        /// <remarks>Need to modify so if the filename is set, the MIME type gets set as well</remarks>
        public string ContentType { get; set; } = "application/octet-stream";

        /// <summary>The actual file bytes</summary>
        public byte[] Data { get; set; } = new byte[0];

        /// <summary>Quick check the object has everything to be valid</summary>
        /// <remarks>ContentType is optional since it can fallback to checking the filename or assume its a default.</remarks>
        [JsonProperty( "isValid" )]
        public bool IsValid
        {
            get { return !String.IsNullOrWhiteSpace( this.Filename ) && this.Data != null && this.Data.Length > 0; }
        }
    }
}

