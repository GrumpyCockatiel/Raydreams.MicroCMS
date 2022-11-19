using System;
using Microsoft.Extensions.Hosting;
using Raydreams.MicroCMS.IO;

namespace Raydreams.MicroCMS.CLI
{
    /// <summary></summary>
    public class Watcher : BackgroundService
    {
        private readonly object _cflock = new object();
        private readonly object _dflock = new object();

        private readonly IHostApplicationLifetime _hostLifetime;

        private FileSystemWatcher watcher;

        /// <summary></summary>
        /// <param name="repo"></param>
        /// <param name="config"></param>
        /// <param name="hostLifetime"></param>
        public Watcher(ICMSRepository repo, AppConfig config, IHostApplicationLifetime hostLifetime)
        {
            _hostLifetime = hostLifetime ?? throw new ArgumentNullException(nameof(hostLifetime));

            this.Repo = repo;
            this.Config = config;

            // the local root folder to watch
            this.WatchRoot = new DirectoryInfo( Path.Combine( this.Config.LocalRoot ) );

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

        /// <summary></summary>
        /// <remarks>Needs to be a list</remarks>
        protected (String, DateTimeOffset) JustChanged { get; set; } = ( String.Empty, DateTimeOffset.MaxValue );

        /// <summary></summary>
        protected AppConfig Config { get; set; }

        /// <summary></summary>
        protected ICMSRepository Repo { get; set; }

        /// <summary></summary>
        protected DirectoryInfo WatchRoot { get; set; }

        /// <summary></summary>
        protected bool Uploading { get; set; } = false;

        /// <summary></summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override Task<int> ExecuteAsync(CancellationToken stoppingToken)
        {
            int exitCode = 0;

            Console.WriteLine($"Watching {this.WatchRoot.FullName}. Press Ctrl-C to quit.");

            try
            {
                Task.Delay(TimeSpan.FromMinutes(10)).GetAwaiter().GetResult();
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("The job has been killed with CTRL+C");
                exitCode = -1;
            }
            catch (Exception ex)
            {
                //_logger?.LogError(ex, "An error occurred");
                exitCode = 1;
            }
            finally
            {
                _hostLifetime.StopApplication();
            }

            return Task.FromResult<int>(exitCode);
        }

        /// <summary></summary>
        /// <param name="path"></param>
        /// <returns></returns>
        protected bool DoRecent(string path)
        {
            if (String.IsNullOrWhiteSpace(this.JustChanged.Item1))
                return true;

            if ( !path.Equals(this.JustChanged.Item1) )
                return true;

            if ( DateTimeOffset.UtcNow > this.JustChanged.Item2.AddSeconds(2) )
                return true;

            return false;
        }

        /// <summary>Any change or create will overwrite</summary>
        private void OnWatchedFolderChanged(object sender, FileSystemEventArgs e)
        {
            if ( !this.DoRecent(e.FullPath) )
                return;

            lock (_cflock)
            {
                var file = IOHelpers.ReadFile( e.FullPath );

                string diff = this.ResolveRemotePath(e.FullPath, false);

                string etag = this.Repo.UploadFile(file, this.Config.RemoteRoot, diff );

                Console.WriteLine($"{e.ChangeType}: {e.FullPath} {DateTimeOffset.UtcNow:o}");

                this.JustChanged = (e.FullPath, DateTimeOffset.UtcNow);

                Console.WriteLine($"{etag} : Uploaded file {file.Filename}");
            }
        }

        /// <summary></summary>
        private void OnWatchedFolderDeleted(object sender, FileSystemEventArgs e)
        {
            lock (_dflock)
            {
                string diff = this.ResolveRemotePath(e.FullPath, true);

                int results = this.Repo.DeleteFile(this.Config.RemoteRoot, diff );

                Console.WriteLine($"{results}: Deleted file {diff} {DateTimeOffset.UtcNow:o}");
            }
        }

        /// <summary></summary>
        /// <param name="fullLocalPath"></param>
        /// <returns></returns>
        /// <remarks>diff of the watch path and the full path - the file name</remarks>
        protected string ResolveRemotePath(string fullLocalPath, bool includeFile )
        {
            string diff = this.WatchRoot.PathDiff( new FileInfo(fullLocalPath), includeFile );
            diff = diff.Trim(new char[] { ' ', '/', '\\' });

            if (diff.StartsWith(this.Config.LocalPagesDir))
                diff = diff.Substring(this.Config.LocalPagesDir.Length);

            return diff;
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
