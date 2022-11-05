using System;
namespace Raydreams.MicroCMS.Integrate
{
    /// <summary>Loads config values from the App Settings JSON environment files</summary>
    public class TestConfig
    {
        #region [ Fields ]

        #endregion [ Fields ]

        #region [ Properties ]

        /// <summary>What environment is this</summary>
        public string Environment { get; set; } = "DEV";

        /// <summary></summary>
        public string? ConnectionString { get; set; }

        /// <summary>The root blob container name</summary>
		public string Root { get; set; } = "blog";

        #endregion [ Properties ]
    }

}

