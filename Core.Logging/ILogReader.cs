using Core.Logging.Configuration;

namespace Core.Logging
{
    /// <summary>
    /// Describes a log reader
    /// </summary>
    public interface ILogReader
    {
        int HasMessage { get; }

        ILogMessage Receive();

        /// <summary>
        /// Used by factory to create objects of this type
        /// </summary>
        /// <param name="applicationName">application name given to this logger</param>
        /// <param name="logName">log name given to this log</param>
        /// <param name="config">The log readers configuration</param>
        /// <returns></returns>
        object Create(string applicationName, string logName, LoggerConfig config);
    }
}