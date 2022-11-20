using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Raydreams.MicroCMS.Logging
{
    /// <summary>Provider for logging to Azure Tables</summary>
    public sealed class FileLogProvider : ILoggerProvider
    {
        private string _dir = null;
        private string _file = null;

        public FileLogProvider( string dir, string filename = null)
        {
            this._dir = dir;
            this._file = filename;
        }

        public ILogger CreateLogger(string categoryName) => new FileLogger(this._dir, _file);

        public void Dispose()
        {
            return;
        }
    }

    /// <summary>Custom File Logger</summary>
	public class FileLogger : ILogger
	{
        #region [ Fields ]

        /// <summary>file lock</summary>
        private readonly object _fileLock = new object();

        /// <summary>Dir path to the log file</summary>
        private DirectoryInfo _path = null;

        /// <summary>Name of the log file</summary>
        private string _filename = null;

        /// <summary>Character to use to deliminate columns</summary>
        private char _delim = '|';

        #endregion [ Fields ]

        #region [ Constructors ]

        /// <summary></summary>
        /// <param name="dir">Directory to write log files to</param>
        /// <param name="logFile">Log File Name</param>
        public FileLogger(string dir, string filename = null )
        {
            this._path = new DirectoryInfo(dir);
            this.LogFilename = filename;
        }

        #endregion [ Constructors ]

        /// <summary>The name of the log file</summary>
        public string LogFilename
        {
            get
            {
                return (String.IsNullOrWhiteSpace(this._filename)) ? DefaultFilename : this._filename;
            }
            set { this._filename = value; }
        }

        /// <summary>A fallback filename if none is provided which creates one log per day</summary>
		public string DefaultFilename => $"log_{DateTimeOffset.UtcNow.ToString("yyyyMMdd")}.txt";

        /// <summary>Get the delimiter character</summary>
		public char Delimiter => this._delim;

        /// <summary>When true, if the path does not exist, it will be created.
        /// If false, and the path does not exist, no log will be written.
        /// </summary>
        public bool Create { get; set; } = true;

        /// <summary>The directory to log to</summary>
		/// <remarks>Very bad name for this since it conflicts with System.IO.Path</remarks>
		public DirectoryInfo Path
        {
            get { return this._path; }
        }

        /// <summary>Get the full file path to the log file</summary>
        public FileInfo FullPath => new FileInfo(System.IO.Path.Combine(this.Path.FullName, this.LogFilename));

        #region [ ILogger ]

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            this.InsertLog(logLevel, null, state.ToString(), null);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state) => default!;

        #endregion [ ILogger ]

        /// <summary>Base logging method</summary>
		/// <param name="logger">The source of the source such as the application name.</param>
		/// <param name="lvl">The std log level as defined by Log4j</param>
		/// <param name="category">An application specific category that can be used for further organization, or routing to differnt locations/</param>
		/// <param name="msg">The actual message to log</param>
		/// <param name="args">any additional data fields to append to the log message. Used for debugging.</param>
		/// <returns></returns>
		protected bool InsertLog(LogLevel lvl, string category, string msg, params object[] args)
        {
            StringBuilder sb = new StringBuilder(DateTime.UtcNow.ToString("s"));
            sb.Append(this.Delimiter);

            //if (lvl < this.Level)
                //return false;

            // make sure the parent dir exists
            if (!this.Path.Exists)
            {
                if (this.Create)
                    Directory.CreateDirectory(this.Path.FullName);
                else
                    return false;
            }

            // append level
            sb.AppendFormat("{0}{1}", lvl, this.Delimiter);

            // append category
            if (String.IsNullOrWhiteSpace(category))
                sb.Append($"<none>{this.Delimiter}");
            else
                sb.AppendFormat("{0}{1}", category, this.Delimiter);

            if (!String.IsNullOrWhiteSpace(msg))
                sb.AppendFormat("{0}", msg.Trim());

            // convert the args dictionary to a string and add to the end
            if (args != null && args.Length > 0)
                //sb.AppendFormat( "|args={0}", String.Join( ";", args ) );
                sb.AppendFormat("{0}{1}", this.Delimiter, String.Join(";", args));

            // write log
            lock (_fileLock)
            {
                StreamWriter osw = null;

                try
                {
                    // open file
                    using (osw = new StreamWriter(this.FullPath.FullName, true))
                    {
                        osw.WriteLine(sb.ToString());
                    }
                }
                catch (System.Exception exp)
                {
                    throw exp;
                }
                finally
                {
                    if (osw != null)
                        osw.Close();
                }
            }

            return true;
        }
    }
}

