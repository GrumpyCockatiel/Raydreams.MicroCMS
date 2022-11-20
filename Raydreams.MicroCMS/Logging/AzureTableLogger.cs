using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Raydreams.MicroCMS.Logging
{
	/// <summary>Provider for logging to Azure Tables</summary>
    public sealed class AzureTableLoggerProvider : ILoggerProvider
    {
        private string _connStr = null;

        public AzureTableLoggerProvider(string connStr)
        {
            this._connStr = connStr;
        }

        public ILogger CreateLogger(string categoryName) => new AzureTableLogger(this._connStr);

        public void Dispose()
        {
            return;
        }
    }

    /// <summary>Logs to an Azure Table</summary>
    public class AzureTableLogger : AzureTableRepository<LogRecord>, ILogger
	{
		private LogLevel _level = LogLevel.Trace;

		private string _src = null;

		#region [Constructors]

		/// <summary>Constructor with a hard coded table name</summary>
		public AzureTableLogger( string connStr, string src = null ) : base( connStr, "Logs" )
		{
			this.Source = src;
		}

		#endregion [Constructors]

		#region [Properties]

		/// <summary>The minimum level to log which defaults to All</summary>
		public LogLevel Level
		{
			get { return this._level; }
			set { this._level = value; }
		}

		/// <summary>The logging source. Who is doing the logging.</summary>
		public string Source
		{
			get { return this._src; }
			set
			{
				if ( value != null )
					this._src = value.Trim();
			}
		}

		#endregion [Properties]

		/// <summary></summary>
		/// <param name="top"></param>
		/// <returns></returns>
		/// <remarks>querying and sorting Azure Tables is not feasible, get the last 7 days and take only the top 100</remarks>
		public async Task<List<LogRecord>> ListTop( int top = 100 )
		{
			// subtract 7 days from today
			DateTimeOffset since = DateTimeOffset.UtcNow.Subtract( new TimeSpan( 7, 0, 0, 0 ) );

			TableQuerySegment<LogRecord> data = null;
			TableContinuationToken tok = new TableContinuationToken();

			// get everything in the past N days
			// if you reached top you could keep looping over the token moving the date back
			var query = new TableQuery<LogRecord>().Where( TableQuery.GenerateFilterConditionForDate( "Timestamp",
				QueryComparisons.GreaterThanOrEqual, since ) );
			data = await this.AzureTable.ExecuteQuerySegmentedAsync<LogRecord>( query, tok );

			// only get the last top
			return data.Results.Take( top ).ToList();
		}

		/// <summary></summary>
		/// <param name="begin"></param>
		/// <param name="end"></param>
		/// <returns></returns>
		/// <remarks>Has not been tested</remarks>
		public async Task<List<LogRecord>> ListByRange( DateTimeOffset begin, DateTimeOffset end )
		{
			TableQuerySegment<LogRecord> data = null;
			TableContinuationToken tok = new TableContinuationToken();

			// needs to loop over the token to be truely correct
			var query = new TableQuery<LogRecord>().Where( TableQuery.GenerateFilterConditionForDate( "Timestamp",
				QueryComparisons.GreaterThanOrEqual, begin ) ).Where( TableQuery.GenerateFilterConditionForDate( "Timestamp",
				QueryComparisons.LessThanOrEqual, end ) );
			data = await this.AzureTable.ExecuteQuerySegmentedAsync<LogRecord>( query, tok );

			return data.Results.ToList();
		}

		/// <summary>Deletes any logs older than the specified number of days</summary>
		/// <param name="days">Age of an old record in days this days = 30 removes all record 30+ days old</param>
		/// <returns>Records removed which will max out at 1000 for now</returns>
		/// <remarks>The returned value has not been tested</remarks>
		public async Task<long> PurgeAfter( int days = 7 )
		{
			if ( days < 0 )
				return 0;

			// subtract days from today
			DateTimeOffset expire = DateTimeOffset.UtcNow.Subtract( new TimeSpan( days, 0, 0, 0 ) );

			TableResult results = null;
			TableQuerySegment<LogRecord> data = null;
			TableContinuationToken tok = new TableContinuationToken();

			// anything with a timestamp less than or equal to today - days is old
			var query = new TableQuery<LogRecord>().Where( TableQuery.GenerateFilterConditionForDate( "Timestamp",
				QueryComparisons.LessThanOrEqual, expire ) );
			
			data = await this.AzureTable.ExecuteQuerySegmentedAsync<LogRecord>( query, tok );

			// perform a delete on each row in the query results
			foreach ( var row in data )
			{
				var op = TableOperation.Delete( row );
				results = await this.AzureTable.ExecuteAsync( op );
				//results.HttpStatusCode
			}

			return data.Results.Count;
		}

		/// <summary>Removes every record in the logger</summary>
		/// <remarks>This function is very dangerous and should be moved to protected at least</remarks>
		protected void Clear()
		{
			base.DeleteAll();
		}

        #region [ ILogger ]

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
			this.InsertLog(logLevel, null, state.ToString(), null );
        }

        public bool IsEnabled(LogLevel logLevel)
        {
			return true;
        }

        public IDisposable BeginScope<TState>(TState state) => default!;

        #endregion [ ILogger ]

        /// <summary></summary>
        protected int InsertLog( LogLevel lvl, string category, string msg, params object[] args )
		{
			if ( lvl < this.Level )
				return 0;

			if ( String.IsNullOrWhiteSpace( this.Source ) )
				this.Source = String.Empty;

			if ( String.IsNullOrWhiteSpace( msg ) )
				msg = String.Empty;

			LogRecord rec = new LogRecord
			{
				Source = this.Source,
				Level = lvl,
				Message = msg,
				Category = category,
				Args = args
			};

			return base.Insert( rec );
		}
	}
}
