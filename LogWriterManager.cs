#region References

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using Civic.Core.Configuration;
using Civic.Core.Logging.Configuration;
using Newtonsoft.Json;

#endregion References

namespace Civic.Core.Logging
{
    /// <summary>
    /// Primary logging class.  This does all of the routing to the log writers
    /// </summary>
    public class LoggerWriterManager
    {
        #region Fields

        private static Dictionary<string,string> _failureFilenames = new Dictionary<string, string>();
        private static readonly object _filenameLock = new object();

        private static readonly List<LoggerConfig> _logWriters = new List<LoggerConfig>();
        private static readonly object _lock = new object();

        #endregion Fields

        #region Properties

        #endregion Properties

        /// <summary>
        /// The name of the directory where log write failures are logged
        /// </summary>
        public static string RecoveryDirectory
        {
            get { return Path.GetTempPath() + Path.DirectorySeparatorChar + "Civic.Logging"; }
        }

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


        /// <summary>
        /// Locates the configuration and creates all of the log writers configured
        /// </summary>
        public static void Init()
        {
            lock (_lock)
            {
                var config = LoggingConfig.Current;
                if(config==null) throw  new ConfigurationErrorsException(string.Format("missing configuration section {0}",LoggingConfig.SectionName));

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

        /// <summary>
        /// Writes a log entry to all configured log writers
        /// </summary>
        /// <param name="message">The log entry to write</param>
        /// <returns>returns true if all were successfully written</returns>
        public static bool Write(ILogMessage message)
        {
            var allSuccess = true;

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
                        if (writerConfig.UseFailureRecovery) // if log recovery is on, than check to see if there are any that need to be recovered
                        {
                            RecoverFailures();
                        }
                    }
                    else
                    {
                        allSuccess = false;
                        if (writerConfig.UseFailureRecovery) // log the write failure, if we are requested too
                        {
                            WriteFailure(message, writerConfig.Name);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(LoggingBoundaries.ServiceBoundary, "Failed to log to writer {0}", ex);
                }
            }

            return allSuccess;
        }

        /// <summary>
        /// Checks to see if any write failures have been logged to the file system.
        /// If found it attempts to write them again
        /// </summary>
        public static void RecoverFailures()
        {
            try
            {
                if (Directory.Exists(RecoveryDirectory))
                {
                    var files = Directory.GetFiles(RecoveryDirectory);
                    foreach (var filename in files)
                    {
                        var parts = filename.Split('_');

                        LoggerConfig logger = null;

                        lock (_logWriters)
                        {
                            foreach (LoggerConfig logWriter in _logWriters)
                            {
                                if (string.Compare(logWriter.Name, parts[0], StringComparison.InvariantCultureIgnoreCase) == 0) continue;
                                logger = logWriter;
                                break;
                            }
                        }

                        if (logger != null)
                        {
                            var success = true;
                            GenerateLogFileName(parts[0], true);
                            using (var file = File.Open(filename, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                            {
                                using (var reader = new StreamReader(file))
                                {
                                    while (!reader.EndOfStream)
                                    {
                                        var entry = reader.ReadLine();
                                        var logEntry = JsonConvert.DeserializeObject<LogMessage>(entry);
                                        if (logEntry == null || logger.Writer.Log(logEntry)) continue;

                                        // failed to write again, so recover the remaining log entries
                                        success = false;
                                        WriteFailure(logEntry, logger.Name);

                                        //move the remaining contents of the log to the new file
                                        while (!reader.EndOfStream)
                                        {
                                            entry = reader.ReadLine();
                                            logEntry = JsonConvert.DeserializeObject<LogMessage>(entry);
                                            if (logEntry != null) WriteFailure(logEntry, logger.Name);
                                        }
                                        break;
                                    }
                                }
                            }
                            if (File.Exists(filename)) File.Delete(filename);
                        }
                    }

                    if (Directory.GetFiles(RecoveryDirectory).Length == 0)
                    {
                        if (Directory.Exists(RecoveryDirectory)) Directory.Delete(RecoveryDirectory);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(LoggingBoundaries.ServiceBoundary, "Failed while too perform log recovery: {0}", ex);
            }
        }

        /// <summary>
        /// Writes a message to a log writers failure log
        /// </summary>
        /// <param name="message">The log entry that filed to write</param>
        /// <param name="name">name of log writer we are logging failure</param>
        private static void WriteFailure(ILogMessage message, string name)
        {
            if (!Directory.Exists(RecoveryDirectory)) Directory.CreateDirectory(RecoveryDirectory);

            var filename = GenerateLogFileName(name, false);
            var entry = JsonConvert.SerializeObject(message, Formatting.None);
            if (entry.Contains("\r")) entry = entry.Replace("\r", "\\r");
            if (entry.Contains("\n")) entry = entry.Replace("\n", "\\n");
            File.AppendAllLines(filename, new[] {entry});
        }

        /// <summary>
        /// Generates a unique log file name for a specific log writer and creates log folder if necessary
        /// </summary>
        /// <param name="name">name of log writer we are creating a filename for</param>
        /// <param name="forceNew">true if a new filename should be force to be generated</param>
        /// <returns></returns>
        private static string GenerateLogFileName(string name, bool forceNew)
        {
            name = name.ToUpperInvariant();
            lock (_filenameLock)
            {
                if (!_failureFilenames.ContainsKey(name) || forceNew)
                {
                    _failureFilenames[name] = RecoveryDirectory + Path.DirectorySeparatorChar + name + "_" + Path.GetRandomFileName();
                }
                name = _failureFilenames[name];
            }
            return name;
        }

        #endregion Methods
    }
}