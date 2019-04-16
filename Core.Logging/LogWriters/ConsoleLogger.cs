#region Copyright / Comments

// <copyright file="ConsoleLogger.cs" company="Civic Engineering & IT">Copyright � Civic Engineering & IT 2013</copyright>
// <author>Chris Doty</author>
// <email>dotyc@civicinc.com</email>
// <date>6/4/2013</date>
// <summary></summary>

#endregion Copyright / Comments

#region References

using System;
using System.Threading.Tasks;
using Stack.Core.Logging.Configuration;

#endregion References

namespace Stack.Core.Logging.LogWriters
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
        /// <param name="applicationname">application name given to this logger</param>
        /// <param name="logname">log name given to this log</param>
        /// <param name="config">The log writers configuration</param>
        /// <returns></returns>
        public object Create(string applicationname, string logname, LoggerConfig config)
        {
            var dl = new ConsoleLogger
                {
                    ApplicationName = applicationname, 
                    LogName = logname
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