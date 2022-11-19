using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Raydreams.MicroCMS.IO;

namespace Raydreams.MicroCMS.CLI
{
    /// <summary></summary>
    public class Watcher : BackgroundService
    {
        /// <summary>lock on uploading a file</summary>
        private readonly object _cflock = new object();

        /// <summary>lock on deleting a file</summary>
        private readonly object _dflock = new object();

        /// <summary></summary>
        private readonly IHostApplicationLifetime _hostLifetime;

        protected FileSystemWatcher Agent { get; set; }

        /// <summary></summary>
        /// <param name="repo"></param>
        /// <param name="config"></param>
        /// <param name="hostLifetime"></param>
        public Watcher(ICMSRepository repo, AppConfig config, ILogger<Watcher> logger, IHostApplicationLifetime hostLifetime)
        {
            _hostLifetime = hostLifetime ?? throw new ArgumentNullException( nameof(hostLifetime) );

            this.Repo = repo;
            this.Config = config;
            this.Logger = logger;

            // the local root folder to watch
            this.WatchRoot = new DirectoryInfo( Path.Combine( this.Config.LocalRoot ) );

            if (!this.WatchRoot.Exists)
                throw new System.ArgumentException("Path to watch is required");

            this.Agent = new FileSystemWatcher(this.WatchRoot.FullName);

            this.Agent.NotifyFilter = NotifyFilters.Attributes
                                 | NotifyFilters.CreationTime
                                 | NotifyFilters.FileName
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.Security
                                 | NotifyFilters.Size;

            this.Agent.Filter = "*.*";
            this.Agent.IncludeSubdirectories = true;
            this.Agent.EnableRaisingEvents = true;

            this.Agent.Changed += OnWatchedFolderChanged;
            this.Agent.Created += OnWatchedFolderChanged;
            this.Agent.Deleted += OnWatchedFolderDeleted;
            //watcher.Renamed += OnWatchedFolderChanged;
            this.Agent.Error += OnError;
        }

        #region [ Properties ]

        /// <summary></summary>
        protected ILogger<Watcher> Logger { get; set; }

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

        #endregion [ Properties ]

        /// <summary></summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override async Task<int> ExecuteAsync( CancellationToken stoppingToken )
        {
            int exitCode = 0;

            this.Logger.LogInformation($"Watching {this.WatchRoot.FullName}. Press CTRL-C to quit.");

            try
            {
                // blocks here until CTRL-C
                await Task.Delay( Timeout.Infinite, stoppingToken );
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

            return exitCode;
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
