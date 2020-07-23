#region References

using System.Threading.Tasks;
using Core.Logging.Configuration;

#endregion References

namespace Core.Logging
{
    /// <summary>
    /// Describes a log writer
    /// </summary>
    public interface ILogWriter
    {
        #region Properties

        /// <summary>
        /// gets the application name given to this logger
        /// </summary>
        string ApplicationName
        {
            get;
        }

        /// <summary>
        /// true if the ILogWriter supports a delete command
        /// false if it does not
        /// </summary>
        bool CanDelete
        {
            get;
        }

        /// <summary>
        /// gets the log name given to this log
        /// </summary>
        string LogName
        {
            get;
        }

        /// <summary>
        /// gets the display name given to this logger
        /// </summary>
        string Name
        {
            get;
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
        object Create(string applicationName, string logName, LoggerConfig config);

        /// <summary>
        /// On logs that can be deleted.  
        /// This will delete the log
        /// </summary>
        void Delete();

        /// <summary>
        /// forces all items left in logger queue to write out to it's storage device
        /// </summary>
        void Flush();

        /// <summary>
        /// Logs a message to the log class
        /// </summary>
        /// <param name="message">the message to write the the log</param>
        /// <returns>Result of logging activity</returns>
        Task<LogWriterResult> Log(ILogMessage message);

        #endregion Methods
    }
}