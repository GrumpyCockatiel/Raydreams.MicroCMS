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
    /// <summary></summary>
    public class AzureFileShareRepository : ICMSRepository
    {
        public AzureFileShareRepository( string connStr )
        {
            this.ConnectionString = connStr;
        }

        #region [ Properties ]

        /// <summary>Azure Storage connection string</summary>
        public string ConnectionString { get; set; } = String.Empty;

        #endregion [ Properties ]

        /// <summary>Gets a text file from an Azure File Share</summary>
        /// <param name="shareName"></param>
        /// <param name="fileName"></param>
        /// <returns>Just returns the text of the file</returns>
        public PageDetails GetTextFile( string shareName, string fileName )
        {
            string contents = String.Empty;

            // validate input
            if ( String.IsNullOrWhiteSpace( fileName ) || String.IsNullOrWhiteSpace( shareName ) )
                return new PageDetails(contents, DateTimeOffset.MaxValue);

            shareName = shareName.Trim();
            fileName = fileName.Trim();

            // Get a reference to a share and then create it
            ShareClient share = new ShareClient( this.ConnectionString, shareName );

            // check the share exists
            Response<bool> exists = share.Exists();

            if ( !exists.Value )
                return new PageDetails(contents, DateTimeOffset.MaxValue);

        
            var dir = share.GetRootDirectoryClient();
            ShareFileClient file = dir.GetFileClient( fileName );

            // check the file exists
            exists = file.Exists();
            if (!exists.Value)
                return new PageDetails(contents, DateTimeOffset.MaxValue);

            // set options
            ShareFileOpenReadOptions op = new ShareFileOpenReadOptions( false );
            using Stream stream = file.OpenRead( op );

            byte[] data = new byte[stream.Length];
            stream.Read( data, 0, data.Length );
            stream.Close();

            // get the properties
            var prop = file.GetProperties();
            DateTimeOffset? ts = prop?.Value?.LastModified;

            return new PageDetails( Encoding.UTF8.GetString( data ), ts ?? DateTimeOffset.MaxValue );
        }

        /// <summary></summary>
        /// <param name="shareName"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public RawFileWrapper GetRawFile( string shareName, string fileName )
        {
            RawFileWrapper contents = new RawFileWrapper();

            // validate input
            if ( String.IsNullOrWhiteSpace( fileName ) || String.IsNullOrWhiteSpace( shareName ) )
                return contents;

            shareName = shareName.Trim();
            fileName = fileName.Trim();

            // Get a reference to a share and then create it
            ShareClient share = new ShareClient( this.ConnectionString, shareName );

            Response<bool> exists = share.Exists();

            if ( !exists.Value )
                return contents;

            var dir = share.GetRootDirectoryClient();
            ShareFileClient file = dir.GetFileClient( fileName );

            // set options
            ShareFileOpenReadOptions op = new ShareFileOpenReadOptions( false );
            using Stream stream = file.OpenRead( op );

            contents.Filename = file.Name;
            contents.ContentType = MimeTypeMap.GetMimeType( Path.GetExtension( file.Name ) );
            contents.Data = new byte[stream.Length];
            stream.Read( contents.Data, 0, contents.Data.Length );
            stream.Close();

            return contents;
        }

        /// <summary>Gets a list of all files</summary>
        /// <param name="shareName"></param>
        /// <returns></returns>
        public List<string> ListFiles( string shareName, string pattern = null )
        {
            List<string> files = new List<string>();

            // validate input
            if ( String.IsNullOrWhiteSpace( shareName ) )
                return files;

            shareName = shareName.Trim();

            // Get a reference to a share and then create it
            ShareClient share = new ShareClient( this.ConnectionString, shareName );

            Response<bool> exists = share.Exists();

            if ( !exists.Value )
                return files;

            ShareDirectoryClient dir = share.GetRootDirectoryClient();

            return this.GetChildren( share, dir );
        }

        /// <summary></summary>
        /// <param name="file"></param>
        /// <param name="shareName"></param>
        /// <param name="sharePath"></param>
        /// <returns></returns>
        public string UploadFile( RawFileWrapper file, string shareName, string sharePath )
        {
            // validate input
            if (!file.IsValid || String.IsNullOrWhiteSpace(shareName))
                return null;

            // Get a reference to a share and then create it
            ShareClient share = new ShareClient(this.ConnectionString, shareName);

            Response<bool> exists = share.Exists();

            if (!exists.Value)
                return null;

            var dir = share.GetRootDirectoryClient();

            string uploadPath = file.Filename.Trim( new char[] { ' ', '/', '\\' } );

            if ( !String.IsNullOrEmpty( sharePath ) )
            {
                sharePath = sharePath.Trim( new char[] { ' ', '/', '\\' } );
                if (sharePath != String.Empty )
                    uploadPath = $"{sharePath}/{file.Filename}";
            }
               
            Response<ShareFileClient> resp = dir.CreateFile(uploadPath, file.Data.Length);
            var data = new MemoryStream(file.Data);

            Response<ShareFileUploadInfo> resp2 = resp.Value.Upload(data);
            string etag = resp2.Value.ETag.ToString();

            Console.WriteLine( $"{etag} : Uploaded file {file.Filename}");
            return etag;
        }

        /// <summary>Recursive call to get all child files</summary>
        /// <param name="share"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        protected List<string> GetChildren( ShareClient share, ShareDirectoryClient dir )
        {
            List<string> files = new List<string>();

            ShareDirectoryGetFilesAndDirectoriesOptions ops = new ShareDirectoryGetFilesAndDirectoriesOptions();
            Pageable<ShareFileItem> results = dir.GetFilesAndDirectories( ops );

            IEnumerator<ShareFileItem> enu = results?.GetEnumerator();

            while ( enu.MoveNext() )
            {
                // needs to descend into the 
                if ( enu.Current.IsDirectory )
                {
                    ShareDirectoryClient parent = share.GetDirectoryClient( enu.Current.Name );
                    var temp = this.GetChildren( share, parent ).Select( f => f = $"{enu.Current.Name}/{f}" );
                    files.AddRange( temp );
                }
                else
                    files.Add( enu.Current.Name );
            };

            return files;
        }
    }
}

