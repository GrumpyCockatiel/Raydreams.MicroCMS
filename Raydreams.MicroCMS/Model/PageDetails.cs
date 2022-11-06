using System;
using Newtonsoft.Json;

namespace Raydreams.MicroCMS
{
    /// <summary></summary>
    public class PageDetails
    {
        public PageDetails() { }

        public PageDetails(string content, DateTimeOffset last)
        {
            this.Content = content;
            this.LastUpdated = last;
        }

        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty( "name" )]
        public string FileName { get; set; }

        [JsonProperty( "lastUpdated" )]
        public DateTimeOffset LastUpdated { get; set; }
    }
}

