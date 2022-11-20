using System;
using Microsoft.Azure.Cosmos.Table;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Raydreams.MicroCMS.Logging
{
    /// <summary>Encapsulate all log information into a single object</summary>
    /// <remarks>Designed to work with it without Azure Tables</remarks>
    public class LogRecord : ITableEntity
    {
        private Guid _id;

        public LogRecord() : this( null, LogLevel.Information )
        {
        }

        public LogRecord( string message, LogLevel level = LogLevel.Information )
        {
            this.Message = message;
            this.Level = level;

            this.ID = Guid.NewGuid();
            this.PartitionKey = "1";
            this.Timestamp = DateTimeOffset.UtcNow;
        }

        /// <summary>Unique ID of the record</summary>
        public Guid ID
        {
            get => this._id;
            set
            {
                if ( value != Guid.Empty )
                    this._id = value;
            }
        }

        #region [ Azure Table Required Properties ]

        /// <summary>For now always 1</summary>
        public string PartitionKey { get; set; }

        /// <summary>Use the internal GUID ID as the Row Key in Azure Tables</summary>
        public string RowKey
        {
            get { return this._id.ToString(); }
            set
            {
                if ( Guid.TryParse( value, out Guid temp ) )
                    this._id = temp;
            }
        }

        /// <summary>DateTime of the event preferably in UTC</summary>
        public DateTimeOffset Timestamp { get; set; }

        /// <summary></summary>
        public string ETag { get; set; }

        #endregion [ Azure Table Required Properties ]

        /// <summary>What was the source of the log - the app, service, ...</summary>
        public string Source { get; set; }

        /// <summary>Severity</summary>
        /// <remarks>See enumerated LogLevels in Logging</remarks>l" )]
        public LogLevel Level { get; set; }

        /// <summary>An optional category to help orgnaize log events.</summary>
        public string Category { get; set; }

        /// <summary>The actual log message</summary>
        public string Message { get; set; }

        /// <summary>Additional arguments can be passed as a generic array</summary>
        /// <remarks>Need to create a custom BSON serializer to conver to string[]</remarks>
        public object[] Args { get; set; }

        /// <summary></summary>
        public void ReadEntity( IDictionary<string, EntityProperty> props, OperationContext operationContext )
        {
            this.Source = props["Source"].StringValue;
            this.Message = props["Message"].StringValue;
            this.Level = props["Level"].StringValue.GetEnumValue<LogLevel>(LogLevel.Information, true );
            this.Category = props["Category"].StringValue;
            string allArgs = props["Args"].StringValue;
            this.Args = allArgs.Split( ';', StringSplitOptions.RemoveEmptyEntries );
        }

        /// <summary></summary>
        public IDictionary<string, EntityProperty> WriteEntity( OperationContext operationContext )
        {
            var props = new Dictionary<string, EntityProperty>
            {
                ["Source"] = new EntityProperty( this.Source ?? String.Empty ),
                ["Message"] = new EntityProperty( this.Message ),
                ["Level"] = new EntityProperty( this.Level.ToString() ),
                ["Category"] = new EntityProperty( this.Category ?? String.Empty ),
                ["Args"] = new EntityProperty( ( this.Args != null && this.Args.Length > 0 ) ? String.Join( ";", this.Args ) : String.Empty )
            };

            return props;
        }
    }
}
