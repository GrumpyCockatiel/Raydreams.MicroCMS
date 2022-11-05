using System;
using System.Collections.Generic;

namespace Raydreams.MicroCMS
{
    /// <summary>Maps extensions to Mime Types</summary>
    public static class MimeTypeMap
    {
        #region [ Fields ]

        /// <summary>the default mime type to use if no matches</summary>
        public static string DefaultMIMEType = "text/plain";

        /// <summary>The dictionary of mime types</summary>
        private static readonly Lazy<IDictionary<string, string>> _mappings = new Lazy<IDictionary<string, string>>( BuildMappings );

        /// <summary>Alter the default MIME type</summary>
        /// <param name="defMime"></param>
        public static void SetDefault( string defMime )
        {
            DefaultMIMEType = !String.IsNullOrWhiteSpace( defMime ) ? defMime : "text/plain";
        }

        #endregion [ Fields ]

        /// <summary>Test the mime type is of type text</summary>
        /// <param name="mimeType"></param>
        /// <returns></returns>
        public static bool IsText( string mimeType ) => mimeType.StartsWith( "text", StringComparison.OrdinalIgnoreCase )
            || mimeType.Equals( GetMimeType( ".json" ), StringComparison.OrdinalIgnoreCase )
            || mimeType.Equals( GetMimeType( ".xml" ), StringComparison.OrdinalIgnoreCase );

        /// <summary>Simple test the file type is supported</summary>
        /// <param name="ext">The file extension optionally prefixed with a .</param>
        /// <returns>True if supposrted</returns>
        public static bool Supported( string ext )
        {
            // validate the input
            if ( String.IsNullOrWhiteSpace( ext ) )
                throw new System.ArgumentNullException( nameof( ext ), "No extension passed." );

            // always use a . prefix
            if ( !ext.StartsWith( "." ) )
                ext = $".{ext}";

            // return a default if no extension found
            return _mappings.Value.ContainsKey( ext );
        }

        /// <summary>Gets the actual MIME Type based on the file extension</summary>
        /// <param name="extension">file extension optionally prefixed with a .</param>
        /// <returns>The mime type or the default mime type if not found</returns>
        public static string GetMimeType( string ext )
        {
            // validate the input
            if ( String.IsNullOrWhiteSpace( ext ) )
                throw new System.ArgumentNullException( nameof( ext ), "No extension passed." );

            // always use a . prefix
            if ( !ext.StartsWith( "." ) )
                ext = $".{ext}";

            // return a default if no extension found
            return _mappings.Value.TryGetValue( ext, out string mime ) ? mime : DefaultMIMEType;
        }

        /// <summary>Does a reverse lookup by mime type for the associated extension</summary>
        public static string GetExtension( string mimeType )
        {
            if ( String.IsNullOrWhiteSpace( mimeType ) )
                throw new System.ArgumentNullException( nameof( mimeType ), "No MIME type passed." );

            if ( !mimeType.Contains( '/' ) )
                throw new System.ArgumentException( $"MIME type is not valid: {mimeType}" );

            if ( _mappings.Value.TryGetValue( mimeType, out string ext ) )
                return ext;

            throw new System.ArgumentException( $"Requested mime type was not found: {mimeType}" );
        }

        /// <summary>Build the supported extensions</summary>
        /// <returns>Lazy loaded list of extensions</returns>
        /// <remarks>Added extensions to support to this list</remarks>
        private static IDictionary<string, string> BuildMappings()
        {
            // dictionary built to lookup using ignore case
            // comment out any types you don't want to support
            return new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase )
            {
                {".bmp", "image/bmp"},
                {".css", "text/css"},
                {".csv", "text/csv" },
                {".gif", "image/gif" },
                {".htm", "text/html"},
                {".html", "text/html"},
                {".ico", "image/x-icon"},
                {".jpg", "image/jpg" },
                {".jpeg","image/jpg" },
                {".js","text/javascript" },
                {".json", "application/json"},
                {".md", "text/html"},
                {".markdown", "text/html"},
                {".pdf", "application/pdf" },
                {".png", "image/png" },
                {".tif", "image/tiff" },
                {".tiff", "image/tiff" },
                {".txt", "text/plain" },
                {".xml", "application/xml" },
                {".zip", "application/zip" }
            };
        }
    }
}

