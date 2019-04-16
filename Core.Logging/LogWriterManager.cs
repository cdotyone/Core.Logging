#region References

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Core.Configuration.Framework;
using Newtonsoft.Json;
using Stack.Core.Logging.Configuration;

#endregion References

namespace Stack.Core.Logging
{
    /// <summary>
    /// Primary logging class.  This does all of the routing to the log writers
    /// </summary>
    public class LoggerWriterManager
    {
        #region Fields

        private static readonly Dictionary<string,string> _failureFilenames = new Dictionary<string, string>();
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
            get { return Path.Combine(Path.GetTempPath(),"Civic.Logging"); }
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
                if(config==null) throw  new ConfigurationErrorsException($"missing configuration section {LoggingConfig.SectionName}");

                if(_logWriters.Count>0) return;

                // load the loggers
                foreach (LoggerConfig logger in config.Writers)
                {
                    var logwriter = DynamicInstance.CreateInstance<ILogWriter>(logger.Assembly, logger.Type);
                    var obj = logwriter.Create(config.ApplicationName, config.LogName, logger) as ILogWriter;
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

            lock (_logWriters)
            {
                if(_logWriters.Count==0) Init();
            }

            // ReSharper disable once InconsistentlySynchronizedField
            foreach (var writerConfig in _logWriters)
            {
                try
                {
                    if (!(writerConfig.ExcludeSeverity.Count == 0 || writerConfig.ExcludeSeverity.Contains(message.Type.ToString())))
                        continue;

                    if (!(writerConfig.ExcludeBoundary.Count == 0 || writerConfig.ExcludeBoundary.Contains(message.Boundary.ToString())))
                        continue;

                    var task = writerConfig.Writer.Log(message);
                    task.ContinueWith(completed =>
                    {
                        var result = completed.Result;
                        if (!result.Success)
                        {
                            allSuccess = false;
                            WriteFailure(result.Message, result.Name);
                        }
                    });
                    task.Start();
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
        public static void RecoverFailures(string name)
        {
            try
            {
                if (Directory.Exists(RecoveryDirectory))
                {
                    var files = Directory.GetFiles(RecoveryDirectory);
                    foreach (var filename in files)
                    {
                        var parts = Path.GetFileNameWithoutExtension(filename).Split('_');

                        LoggerConfig logger = null;

                        lock (_logWriters)
                        {
                            foreach (LoggerConfig logWriter in _logWriters)
                            {
                                if (string.Compare(logWriter.Name, parts[0], StringComparison.InvariantCultureIgnoreCase) != 0) continue;
                                logger = logWriter;
                                break;
                            }
                        }
                        
                        if (logger != null && (name==null || string.Compare(logger.Name,name, StringComparison.InvariantCultureIgnoreCase) ==0 ))
                        {
                            GenerateLogFileName(parts[0], true);
                            using (var file = File.Open(filename, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                            {
                                using (var reader = new StreamReader(file))
                                {
                                    while (!reader.EndOfStream)
                                    {
                                        var entry = Base64Decode(reader.ReadLine());
                                        var logEntry = JsonConvert.DeserializeObject<LogMessage>(entry);
                                        if (logEntry == null) continue;

                                        var task = logger.Writer.Log(logEntry);
                                        task.Start();
                                        Task.WaitAll(new Task[] {task});
                                    }
                                }
                            }
                            if (File.Exists(filename)) File.Delete(filename);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(LoggingBoundaries.ServiceBoundary, "Failed while trying too perform log recovery: {0}", ex);
            }
        }

        /// <summary>
        /// Writes a message to a log writers failure log
        /// </summary>
        /// <param name="message">The log entry that filed to write</param>
        /// <param name="name">name of log writer we are logging failure</param>
        public static void WriteFailure(ILogMessage message, string name)
        {
            int count = 0;

            again:

            try
            {
                if (!Directory.Exists(RecoveryDirectory)) Directory.CreateDirectory(RecoveryDirectory);

                var filename = GenerateLogFileName(name, count>0);
                count++;
                var entry = Base64Encode(JsonConvert.SerializeObject(message, Formatting.None));
                File.AppendAllLines(filename, new[] {entry});
            }
            catch
            {
                if (count < 2) goto again;
                else throw;
            }
        }

        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        private static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
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
                    _failureFilenames[name] = Path.Combine(RecoveryDirectory,name + "_" + Path.GetRandomFileName());
                }
                name = _failureFilenames[name];
            }
            return name;
        }

        #endregion Methods
    }
}