using System;
using CommandLine;
using CommandLine.Text;
using Newtonsoft.Json;

namespace Raydreams.MicroCMS.CLI
{
    /// <summary>Options that apply to all the apps</summary>
    public class CLIBaseOptions
    {
        /// <summary></summary>
        [Option('w', "watch", Required = false, HelpText = "")]
        public string? WatchRoot { get; set; }
    }
}
