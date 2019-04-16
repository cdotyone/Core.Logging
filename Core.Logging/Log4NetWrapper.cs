#region Copyright / Comments

// <copyright file="log4net.cs" company="Civic Engineering & IT">Copyright © Civic Engineering & IT 2013</copyright>
// <author>Chris Doty</author>
// <email>dotyc@civicinc.com</email>
// <date>6/4/2013</date>
// <summary></summary>

#endregion Copyright / Comments

#region References

using System;
using Stack.Core.Logging;

#endregion References

// this namespace is used to pump log4net messages into this logger framework
// ReSharper disable CheckNamespace
namespace log4net
// ReSharper restore CheckNamespace
{
    /// <summary>
    /// this is used for log4net compatiblity
    /// </summary>
    public class Log4NetWrapper : ILog
    {
        #region Properties

        public bool IsDebugEnabled
        {
            get { return Logger.IsTraceOn; }
        }

        public bool IsErrorEnabled
        {
            get { return true; }
        }

        public bool IsInfoEnabled
        {
            get { return true; }
        }

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
            if ( ex is Exception )
                Logger.Log(LogMessage.LogTrace(LoggingBoundaries.Unknown, message.Replace("{0}", ((Exception)ex).Message)));
            else
                Logger.Log(LogMessage.LogTrace(LoggingBoundaries.Unknown, message.Replace("{0}", ex.ToString())));
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