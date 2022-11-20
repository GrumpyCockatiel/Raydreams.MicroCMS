using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Raydreams.MicroCMS.IO;

namespace Raydreams.MicroCMS.CLI
{
    /// <summary></summary>
    public class Watcher : BackgroundService
    {
        #region [ Fields ]

        /// <summary>lock on changing any remote file</summary>
        private readonly object _flock = new object();

        /// <summary></summary>
        private readonly IHostApplicationLifetime _hostLifetime;

        /// <summary></summary>
        private readonly string[] ExclusionList = new[] { "DS_Store", "log_" };

        #endregion [ Fields ]

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
            this.Agent.Renamed += OnWatchedFolderRenamed;
            this.Agent.Error += OnError;
        }

        #region [ Properties ]

        /// <summary>The actual folder watcher</summary>
        protected FileSystemWatcher Agent { get; set; }

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
                // when CTRL-C is pressed the cancellation token with be set to cancel and this task will stop
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
            // some files we dont want to upload
            if ( String.IsNullOrWhiteSpace(e.Name) || e.Name.Contains( this.ExclusionList ) )
                return;

            if ( !this.DoRecent(e.FullPath) )
                return;

            this.UpdateRemoteFile(e.FullPath);
        }

        /// <summary></summary>
        private void OnWatchedFolderDeleted(object sender, FileSystemEventArgs e)
        {
            this.DeleteRemoteFile(e.FullPath);
        }

        /// <summary></summary>
        private void OnWatchedFolderRenamed(object sender, RenamedEventArgs e)
        {
            // do a delete on the old file
            this.DeleteRemoteFile(e.OldFullPath);

            // insert on the new file
            this.UpdateRemoteFile(e.FullPath);
        }

        /// <summary></summary>
        /// <param name="fullLocalPath"></param>
        /// <returns></returns>
        protected bool UpdateRemoteFile(string fullLocalPath)
        {
            lock (_flock)
            {
                var file = IOHelpers.ReadFile(fullLocalPath);

                string diff = this.ResolveRemotePath(fullLocalPath, false);

                string etag = this.Repo.UploadFile(file, this.Config.RemoteRoot, diff);

                // check if this file was recently uploaded
                this.JustChanged = (fullLocalPath, DateTimeOffset.UtcNow);

                Console.WriteLine($"{etag} : Uploaded file {file.Filename}");

                return !String.IsNullOrWhiteSpace(etag);
            }
        }

        /// <summary></summary>
        /// <param name="fullLocalPath"></param>
        /// <returns></returns>
        protected bool DeleteRemoteFile(string fullLocalPath)
        {
            if (String.IsNullOrWhiteSpace(fullLocalPath))
                return false;

            lock (_flock)
            {
                int results = 0;

                string diff = this.ResolveRemotePath(fullLocalPath, true);

                results = this.Repo.DeleteFile(this.Config.RemoteRoot, diff);

                Console.WriteLine($"{results}: Deleted file {diff} {DateTimeOffset.UtcNow:o}");

                return results > 0;
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
            this.Logger.LogError(e.GetException(), e.GetException().Message);
        }

    }

}
