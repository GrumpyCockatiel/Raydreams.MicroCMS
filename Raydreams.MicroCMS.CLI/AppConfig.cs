using System;
using System.Runtime.InteropServices;

namespace Raydreams.MicroCMS.CLI
{
	public class AppConfig
	{
        #region [ Properties ]

        /// <summary>Storage connection string</summary>
        public string? ConnectionString { get; set; }

        /// <summary>The root container or share name of the entire site</summary>
        public string RemoteRoot { get; set; } = "blog";

        /// <summary>The root of the Local Folder to watch</summary>
        /// <remarks>Get from the command line -w switch</remarks>
		public string LocalRoot { get; set; } = "Blog";

        /// <summary></summary>
        /// <remarks>Don't change for now but making ocnfigurable</remarks>
		public string LocalPagesDir { get; set; } = "page";

        /// <summary></summary>
        /// <remarks>Don't change for now but making ocnfigurable</remarks>
        public string LocalImagesDir { get; set; } = "image";

        /// <summary></summary>
        /// <remarks>Don't change for now but making ocnfigurable</remarks>
        public string LocalLayoutsDir { get; set; } = "layouts";

        #endregion [ Properties ]
    }

    /// <summary>Simple OS test extensions</summary>
    public static class EnvironmentExtensions
    {
        public static bool IsWindows() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        public static bool IsMacOS() => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        public static bool IsLinux() => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
    }
}

