using System;

namespace Raydreams.MicroCMS
{
    public class ClientHeaders
    {
        /// <summary>The clients IP address</summary>
        private string _ip = "0.0.0.0";

        #region [ Properties ]

        /// <summary>Authorization token sent by the client</summary>
        public string AccessToken { get; set; }

        /// <summary>Store the client agent</summary>
        public string ClientAgent { get; set; } = "no client";

        /// <summary>Store the Client IP from the Request</summary>
        public string ClientIP
        {
            get => !String.IsNullOrWhiteSpace( this._ip ) ? this._ip.Trim() : "0.0.0.0";
            set => this._ip = value;
        }

        #endregion [ Properties ]
    }
}

