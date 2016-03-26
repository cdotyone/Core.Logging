#region References

using System;
using System.Collections.Generic;
using System.Configuration;
using Civic.Core.Configuration;
using Civic.Core.Logging.Configuration;

#endregion References

namespace Civic.Core.Logging
{
    /// <summary>
    /// Primary logging class.  This does all of the routing to the log writers
    /// </summary>
    public class LoggerWriterManager
    {
        #region Fields

        private static readonly List<LoggerConfig> _logWriters = new List<LoggerConfig>();
        private static readonly object _lock = new object();

        #endregion Fields

        #region Methods

        /// <summary>
        /// clears any remaining log entries left to process
        /// </summary>
        public static void Clear()
        {
            lock (_lock)
            {
                _logWriters.Clear();
            }
        }

        public static void Init()
        {
            lock (_lock)
            {
                var config = LoggerSection.Current;
                if(config==null) throw  new ConfigurationErrorsException(string.Format("missing configuration section {0}",LoggerSection.SectionName));

                if(_logWriters.Count>0) return;

                // load the loggers
                foreach (LoggerConfig logger in config.Writers)
                {
                    var logwriter = DynamicInstance.CreateInstance<ILogWriter>(logger.Assembly, logger.Type);
                    var obj = logwriter.Create(config.ApplicationName, config.LogName, config.UseThread, logger.Attributes) as ILogWriter;
                    logger.Writer = obj;
                    _logWriters.Add(logger);
                }
            }
        }

        public static bool Write(ILogMessage message)
        {
            var oneSuccess = false;

            if(_logWriters.Count==0) Init();

            // ReSharper disable once InconsistentlySynchronizedField
            foreach (var writerConfig in _logWriters)
            {
                try
                {
                    if (!(writerConfig.FilterBy.Count == 0 || writerConfig.FilterBy.Contains(message.Type.ToString())))
                        continue;
                    if (writerConfig.Writer.Log(message))
                    {
                        oneSuccess = true;
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(LoggingBoundaries.ServiceBoundary, "Failed to log to writer {0}", ex);
                }
            }

            return oneSuccess;
        }

        #endregion Methods
    }
}