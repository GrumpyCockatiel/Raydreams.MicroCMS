using System;

namespace Raydreams.MicroCMS.CLI
{
	public class AppConfig
	{
        #region [ Properties ]

        /// <summary></summary>
        public string? ConnectionString { get; set; }

        public string RemoteRoot { get; set; } = "blog";

        /// <summary>The root blob container name</summary>
        /// <remarks>Want to get from the command line</remarks>
		public string WatchRoot { get; set; } = "Blog";

        /// <summary></summary>
		public string PagesDir { get; set; } = "page";

        /// <summary></summary>
        public string ImagesDir { get; set; } = "image";

        /// <summary></summary>
        public string LayoutsDir { get; set; } = "layouts";

        #endregion [ Properties ]
    }
}

