using System;

namespace Raydreams.MicroCMS.CLI
{
	public class AppConfig
	{
        #region [ Fields ]

        #endregion [ Fields ]

        #region [ Properties ]

        /// <summary></summary>
        public string? ConnectionString { get; set; }

        /// <summary>The root blob container name</summary>
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

