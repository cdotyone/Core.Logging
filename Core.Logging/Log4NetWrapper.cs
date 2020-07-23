#region References

using System;
using Core.Logging;

#endregion References

// this namespace is used to pump log4net messages into this logger framework
// ReSharper disable CheckNamespace
namespace log4net
// ReSharper restore CheckNamespace
{
    /// <summary>
    /// this is used for log4net compatibility
    /// </summary>
    public class Log4NetWrapper : ILog
    {
        #region Properties

        public bool IsDebugEnabled => Logger.IsTraceOn;

        public bool IsErrorEnabled => true;

        public bool IsInfoEnabled => true;

        #endregion Properties

        #region Methods

        public void Debug(string message)
        {
            Logger.Log(LogMessage.LogTrace(LoggingBoundaries.Unknown, message));
        }

        public void Debug(string message, Exception ex)
        {
            Logger.Log(LogMessage.LogTrace(LoggingBoundaries.Unknown, message + " :: " + ex.Message));
        }

        public void DebugFormat( string message, object ex )
        {
            Logger.Log(ex is Exception exception
                ? LogMessage.LogTrace(LoggingBoundaries.Unknown, message.Replace("{0}", exception.Message))
                : LogMessage.LogTrace(LoggingBoundaries.Unknown, message.Replace("{0}", ex.ToString())));
        }

        public void DebugFormat( string message )
        {
            Logger.Log(LogMessage.LogTrace(LoggingBoundaries.Unknown, message));
        }

        public void Error(string message)
        {
            Logger.Log(LogMessage.LogError(LoggingBoundaries.Unknown, message));
        }

        public void Error(string message, Exception ex)
        {
            Logger.Log(LogMessage.LogError(LoggingBoundaries.Unknown, message + " :: " + ex.Message));
        }

        public void Info(string message)
        {
            Logger.Log(LogMessage.LogInformation(LoggingBoundaries.Unknown, message));
        }

        public void Warn(string message)
        {
            Logger.Log(LogMessage.LogWarning(LoggingBoundaries.Unknown, message));
        }

        #endregion Methods
    }
}