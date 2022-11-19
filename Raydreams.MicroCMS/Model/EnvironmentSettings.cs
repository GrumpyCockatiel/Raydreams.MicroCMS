using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Raydreams.MicroCMS
{
	public enum StoreType
    {
		/// <summary>File Share</summary>
		File = 0,
        /// <summary>Blob Container</summary>
        Blob = 1
    }

	/// <summary>Encapsulates the settings are various environments.</summary>
	public class EnvironmentSettings
	{
        // These match to the keys in the Azure Function Configuation section
        #region [ Config Keys ]

        /// <summary></summary>
        public static readonly string EnvironmentKey = "ASPNETCORE_ENVIRONMENT";

        /// <summary></summary>
        public static readonly string BlobRootKey = "root";

        /// <summary></summary>
        public static readonly string FileStoreKey = "connStr";

        /// <summary></summary>
        public static readonly string ImagesDirKey = "imageDir";

        /// <summary></summary>
        public static readonly string LayoutsDirKey = "layoutDir";

        /// <summary></summary>
        public static readonly string LayoutsExtKey = "layoutExt";

        /// <summary></summary>
        public static readonly string DefaultHomeKey = "homepage";

        /// <summary></summary>
        public static readonly string DefaultErrorKey = "errorpage";

        /// <summary></summary>
        public static readonly string StoreTypeKey = "store";

        #endregion [ Config Keys ]

        /// <summary>Main constructor loads Config settings</summary>
        public EnvironmentSettings()
        {
            // load client details
			this.BlobRoot = Environment.GetEnvironmentVariable( BlobRootKey );
            this.FileStore = Environment.GetEnvironmentVariable( FileStoreKey );

            this.ImagesDir = Environment.GetEnvironmentVariable( ImagesDirKey );
            this.LayoutsDir = Environment.GetEnvironmentVariable( LayoutsDirKey );
            this.LayoutExtension = Environment.GetEnvironmentVariable( LayoutsExtKey );

            this.DefaultHome = Environment.GetEnvironmentVariable( DefaultHomeKey );
            this.DefaultError = Environment.GetEnvironmentVariable( DefaultErrorKey );

			this.Store = Environment.GetEnvironmentVariable( DefaultErrorKey ).GetEnumValue<StoreType>( true );

            this.ExcludeFolders = new string[] { this.ImagesDir, this.LayoutsDir }; 
        }

        /// <summary>Gets environment settings from a string based on the enum value</summary>
        public static EnvironmentSettings GetSettings( string type )
		{
			EnvironmentType env = type.GetEnumValue<EnvironmentType>( EnvironmentType.Unknown );

			EnvironmentSettings set = GetSettings( env );

			set.EnvironmentKeyValue = type;
			return set;
		}

		/// <summary>Gets environment settings from an enum value</summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static EnvironmentSettings GetSettings( EnvironmentType type )
		{
			if ( type == EnvironmentType.Production )
				return PROD;
			else if ( type == EnvironmentType.Development )
				return DEV;
			//else if ( type == EnvironmentType.Testing )
			//return TEST;
			//else if ( type == EnvironmentType.Staging )
			//return STG;
			else
				return DEV;
		}

		#region [ Properties ]

		/// <summary>The enumerated environment type</summary>
		public EnvironmentType EnvironmentType { get; set; }

		/// <summary>The actual key used to load the environment</summary>
		public string EnvironmentKeyValue { get; set; }

		/// <summary>The connection string to the Storage Account</summary>
		public string FileStore { get; set; }

		/// <summary>The root blob container or share name</summary>
		public string BlobRoot { get; set; } = "cms";

        /// <summary>The images folder</summary>
        public string ImagesDir { get; set; } = "image";

        /// <summary>The layouts folder</summary>
        public string LayoutsDir { get; set; } = "layouts";

        /// <summary>The filed extension used for the layout files without a .</summary>
        public string LayoutExtension { get; set; } = "html";

        /// <summary>Default home page MD file</summary>
        public string DefaultHome { get; set; } = "index";

        /// <summary>Default error page MD file</summary>
        public string DefaultError { get; set; } = "error";

		/// <summary></summary>
		public StoreType Store { get; set; } = StoreType.File;

        /// <summary></summary>
        public IEnumerable<string> ExcludeFolders { get; set; }

        #endregion [ Properties ]

        /// <summary>Unknown environment settings</summary>
        public static EnvironmentSettings UNKNOWN
		{
			get
			{
				return new EnvironmentSettings()
				{
					EnvironmentType = EnvironmentType.Unknown
				};
			}
		}

		/// <summary>DEV environment settings</summary>
		public static EnvironmentSettings DEV
		{
			get
			{
				return new EnvironmentSettings()
				{
					EnvironmentType = EnvironmentType.Development
				};
			}
		}

		/// <summary>PROD environment settings</summary>
		public static EnvironmentSettings PROD
		{
			get
			{
				return new EnvironmentSettings()
				{
					EnvironmentType = EnvironmentType.Production
                };

			}
		}
	}
}

