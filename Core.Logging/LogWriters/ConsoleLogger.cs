#region References

using System;
using System.Threading.Tasks;
using Core.Logging.Configuration;

#endregion References

namespace Core.Logging.LogWriters
{
    public class ConsoleLogger : ILogWriter
    {
        #region Properties

        /// <summary>
        /// gets the application name given to this logger
        /// </summary>
        public string ApplicationName { get; private set; }

        /// <summary>
        /// true if the ILogWriter supports a delete command
        /// false if it does not
        /// </summary>
        public bool CanDelete
        {
            get { return false; }
        }

        public string LogName { get; private set; }

        /// <summary>
        /// gets the display name given to this logger
        /// </summary>
        public string Name
        {
            get { return "Console Logger"; }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Used by factory to create objects of this type
        /// </summary>
        /// <param name="applicationName">application name given to this logger</param>
        /// <param name="logName">log name given to this log</param>
        /// <param name="config">The log writers configuration</param>
        /// <returns></returns>
        public object Create(string applicationName, string logName, LoggerConfig config)
        {
            var dl = new ConsoleLogger
                {
                    ApplicationName = applicationName, 
                    LogName = logName
                };

            return dl;
        }

        /// <summary>
        /// Does nothing, required by interface
        /// </summary>
        public void Delete()
        {
        }

        /// <summary>
        /// Does nothing, required by interface
        /// </summary>
        public void Flush()
        {
        }

        /// <summary>
        /// Logs a message to the log class
        /// </summary>
        /// <param name="message">the message to write the the log</param>
        public Task<LogWriterResult> Log(ILogMessage message)
        {
            return new Task<LogWriterResult>(delegate
            {
                if (string.IsNullOrEmpty(message.ApplicationName)) message.ApplicationName = ApplicationName;
                Console.WriteLine(message.Message);
                return new LogWriterResult {Success = true, Name = Name};
            });

        }

        #endregion Methods
    }
}