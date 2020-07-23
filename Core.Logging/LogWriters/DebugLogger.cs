#region References

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Core.Logging.Configuration;

#endregion References

namespace Core.Logging.LogWriters
{
    public class DebugLogger : ILogWriter
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
            get { return true; }
        }

        /// <summary>
        /// gets the log name given to this log
        /// </summary>
        public string LogName { get; private set; }

        /// <summary>
        /// gets the display name given to this logger
        /// </summary>
        public string Name
        {
            get { return "Debug Logger"; }
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
            var dl = new DebugLogger
                {
                    ApplicationName = applicationName,
                    LogName = logName
                };

            return dl;
        }

        /// <summary>
        /// On logs that can be deleted.  
        /// This will delete the log
        /// </summary>
        public void Delete()
        {
            throw new Exception("The method or operation is not supported.");
        }

        /// <summary>
        /// forces all items left in logger queue to write out to it's storage device
        /// </summary>
        public void Flush()
        {
            Debug.Flush();
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
                Debug.AutoFlush = true;

                switch (message.Type)
                {
                    case LogSeverity.Exception:
                        Debug.WriteLine(ApplicationName + " (" + message.Boundary + ") - EXCEPTION: " +
                                        message.Message);
                        break;
                    case LogSeverity.Error:
                        Debug.WriteLine(ApplicationName + " (" + message.Boundary + ") - ERROR: " + message.Message);
                        break;
                    case LogSeverity.Warning:
                        Debug.WriteLine(ApplicationName + " (" + message.Boundary + ") - WARNING: " + message.Message);
                        break;
                    case LogSeverity.Information:
                        Debug.WriteLine(ApplicationName + " (" + message.Boundary + ") - INFORMATION: " +
                                        message.Message);
                        break;
                    case LogSeverity.Trace:
                        Debug.WriteLine(
                            ApplicationName + " (" + message.Boundary + ")" + " - TRACE: " + message.Message);
                        break;
                }

                return new LogWriterResult { Success = true, Name = Name }; ;
            });
        }

        #endregion Methods
    }
}