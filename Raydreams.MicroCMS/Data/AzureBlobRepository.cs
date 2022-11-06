using System;
using System.IO;
using Azure;
using System.Collections.Generic;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Files;
using System.Collections;
using System.Text;
using Azure.Storage.Files.Shares;
using System.Security.Cryptography.X509Certificates;
using Microsoft.VisualBasic;
using System.Reflection.Metadata;
using Azure.Storage.Files.Shares.Models;
using System.Linq;

namespace Raydreams.MicroCMS
{
    /// <summary>Data manager with Azure Files and Blobs.</summary>
    /// <remarks>Blob and Container names are CASE SENSITIVE</remarks>
	public class AzureBlobRepository : ICMSRepository
    {
		#region [ Constructor ]

		/// <summary></summary>
		/// <param name="connStr"></param>
		public AzureBlobRepository( string connStr )
		{
			this.ConnectionString = connStr;
		}

		#endregion [ Constructor ]

		#region [ Properties ]

		/// <summary>Azure Storage connection string</summary>
		public string ConnectionString { get; set; } = String.Empty;

		/// <summary>Used when uploading an image to storage</summary>
		public Stream ImageFile { get; set; }

		/// <summary>Used when uploading an image to storage</summary>
		public string ContentType { get; set; }

		/// <summary>Used when uploading an image to storage</summary>
		/// <remarks>May no longer need this property</remarks>
		public string FileName { get; set; }

		#endregion [ Properties ]

		#region [ Methods ]

		/// <summary>Check to see if a blob already exists in the specified container</summary>
		/// <param name="containerName"></param>
		/// <param name="blobName">File or blob name to check for</param>
		/// <returns></returns>
		/// <remarks>Remember to include a file extension</remarks>
		public bool BlobExists( string containerName, string blobName )
		{
			// blob container name - can we set a default somehow
			if ( containerName == null || blobName == null )
				throw new System.ArgumentNullException( "Arguments can not be null." );

			containerName = containerName.Trim();
			blobName = blobName.Trim();

			if ( containerName == String.Empty || blobName == String.Empty )
				return false;

			// Get a reference to a share and then create it
			BlobContainerClient container = new BlobContainerClient( this.ConnectionString, containerName );

			// check the container exists
			Response<bool> exists = container.Exists();
			if ( !exists.Value )
				return false;

			// Get a reference to the blob name
			BlobClient blob = container.GetBlobClient( blobName );
			exists = blob.Exists();

			return exists.Value;
		}

        /// <summary>Gets a blob from Azure Storage as just raw bytes with metadata</summary>
        /// <param name="containerName">container name</param>
        /// <param name="blobName">blob name</param>
        /// <returns>Wrapped raw bytes with some metadata</returns>
        public RawFileWrapper GetRawFile( string containerName, string blobName )
        {
            RawFileWrapper results = new RawFileWrapper();

            // validate input
            if ( String.IsNullOrWhiteSpace( containerName ) || String.IsNullOrWhiteSpace( blobName ) )
                return results;

            containerName = containerName.Trim();
            blobName = blobName.Trim();

            // Get a reference to a share and then create it
            BlobContainerClient container = new BlobContainerClient( this.ConnectionString, containerName );

            // check the container exists
            Response<bool> exists = container.Exists();
            if ( !exists.Value )
                return results;

            // set options
            BlobOpenReadOptions op = new BlobOpenReadOptions( false );

            // read the blob to an array
            BlobClient blob = container.GetBlobClient( blobName );
            using Stream stream = blob.OpenRead( op );
            results.Data = new byte[stream.Length];
            stream.Read( results.Data, 0, results.Data.Length );
            stream.Close();

            // get the properties
            BlobProperties props = blob.GetProperties().Value;

            if ( props == null )
                return results;

            results.ContentType = props.ContentType;

            // get a filename
            if ( props.Metadata.ContainsKey( "filename" ) )
                results.Filename = props.Metadata["filename"].ToString();
            else
                results.Filename = blob.Name;

            return results;
        }

        /// <summary></summary>
        /// <param name="containerName"></param>
        /// <param name="blobName"></param>
        /// <returns></returns>
        public PageDetails GetTextFile( string containerName, string blobName )
        {
			string contents = String.Empty;

            // validate input
            if ( String.IsNullOrWhiteSpace( containerName ) || String.IsNullOrWhiteSpace( blobName ) )
                new PageDetails( contents, DateTimeOffset.MaxValue );

            containerName = containerName.Trim();
            blobName = blobName.Trim();

            // Get a reference to a share and then create it
            BlobContainerClient container = new BlobContainerClient( this.ConnectionString, containerName );

            // check the container exists
            Response<bool> exists = container.Exists();
            if ( !exists.Value )
                new PageDetails( contents, DateTimeOffset.MaxValue );

            // set options
            BlobOpenReadOptions op = new BlobOpenReadOptions( false );

            // read the blob to an array - BUG the stream could be longer than Int.Max
            BlobClient blob = container.GetBlobClient( blobName );
            using Stream stream = blob.OpenRead( op );
            byte[] data = new byte[stream.Length];
            stream.Read( data, 0, data.Length );
            stream.Close();

            // get the properties
            BlobProperties props = blob.GetProperties().Value;

            contents = Encoding.UTF8.GetString( data );

			return new PageDetails( contents, props.LastModified );
        }

        /// <summary>Get a list of All blobs in the specified contaier</summary>
        /// <param name="containerName">container name</param>
        /// <returns>A list of blob names</returns>
        /// <remarks>Still need to determine what we need back for each blob</remarks>
        public List<string> ListFiles( string containerName, string pattern = null )
		{
			List<string> blobs = new List<string>();

			// blob container name - can we set a default somehow
			if ( String.IsNullOrWhiteSpace( containerName ) )
				return new List<string>();

			// Get a reference to a share and then create it
			BlobContainerClient container = new BlobContainerClient( this.ConnectionString, containerName );

			// check the container exists
			Response<bool> exists = container.Exists();
			if ( !exists.Value )
				return new List<string>();

			Pageable<BlobItem> results = null;
			if ( String.IsNullOrWhiteSpace( pattern ) )
				results = container.GetBlobs();
			else
				results = container.GetBlobs( prefix: pattern.Trim() );

			IEnumerator<BlobItem> enu = results?.GetEnumerator();
			while ( enu.MoveNext() )
			{
				blobs.Add( enu.Current.Name );
			};

			return blobs;
		}

		#endregion [ Methods ]
	}
}
