using System;
using Microsoft.Extensions.Hosting;
using Raydreams.MicroCMS.IO;

namespace Raydreams.MicroCMS.CLI
{
    /// <summary></summary>
    public class Watcher : BackgroundService
    {
        private readonly object _flock = new object();

        private FileSystemWatcher watcher;

        public Watcher(ICMSRepository repo, AppConfig config)
        {
            this.Repo = repo;
            this.Config = config;

            this.WatchRoot = new DirectoryInfo( Path.Combine( this.Config.WatchRoot, this.Config.PagesDir ) );

            if (!this.WatchRoot.Exists)
                throw new System.ArgumentException("Path to watch is required");

            watcher = new FileSystemWatcher(this.WatchRoot.FullName);

            watcher.NotifyFilter = NotifyFilters.Attributes
                                 | NotifyFilters.CreationTime
                                 | NotifyFilters.FileName
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.Security
                                 | NotifyFilters.Size;

            watcher.Changed += OnWatchedFolderChanged;
            watcher.Created += OnWatchedFolderChanged;
            watcher.Deleted += OnWatchedFolderDeleted;
            //watcher.Renamed += OnWatchedFolderChanged;
            watcher.Error += OnError;

            watcher.Filter = "*.*";
            watcher.IncludeSubdirectories = true;
            watcher.EnableRaisingEvents = true;
        }

        protected AppConfig Config { get; set; }

        protected ICMSRepository Repo { get; set; }

        protected DirectoryInfo WatchRoot { get; set; }

        protected bool Uploading { get; set; } = false;

        /// <summary></summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override Task<int> ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine($"Watching {this.WatchRoot.FullName}. Press Ctrl-C to quit.");

            Task.Delay( TimeSpan.FromMinutes(10) ).GetAwaiter().GetResult();

            return Task.FromResult<int>(0);
        }

        /// <summary>Any change or create will overwrite</summary>
        private void OnWatchedFolderChanged(object sender, FileSystemEventArgs e)
        {
            if (this.Uploading)
                return;

            lock (_flock)
            {
                this.Uploading = true;

                var file = IOHelpers.ReadFile(e.FullPath);

                // diff of the watch path and the full path - the file name
                string diff = this.WatchRoot.PathDiff( new FileInfo(e.FullPath), false );

                string etag = this.Repo.UploadFile(file, "blog", diff );

                Console.WriteLine($"{e.ChangeType}: {e.FullPath} {DateTimeOffset.UtcNow:o}");

                this.Uploading = false;
            }
        }

        /// <summary></summary>
        private void OnWatchedFolderDeleted(object sender, FileSystemEventArgs e)
        {

        }

        /// <summary></summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnError(object sender, ErrorEventArgs e)
        {
            Console.WriteLine($"{e.GetException()}");
        }


    }

}

