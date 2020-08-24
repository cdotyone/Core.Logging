#region References

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

#endregion References

namespace Core.Logging
{
    /// <summary>
    /// Primary logging class.  This does all of the routing to the log writers
    /// </summary>
    public static class Logger
    {
        #region Fields

        private static ILogger _logger;
        private static string _environment;
        private static string _applicationName;

        private static readonly IDisposable _dummyTrace = new PerformanceTracerDummy();

        #endregion Fields

        /// <summary>
        /// Default trace level for all of the loggers
        /// If set to -1 the defaults for the loggers are used
        /// The higher the number the more detail that is provided
        /// 
        /// this property should be from -1 to 5
        /// </summary>
        public static bool IsTraceOn {
            get
            {
                return _logger.IsEnabled(LogLevel.Trace);
            }
        }

        #region Logging Methods

        /// <summary>
        /// Initializes the logger library
        /// Normally not called by consuming application, but automatically called
        /// when a log entry is requested.  Can be called to prime the log system
        /// before it is used.
        /// </summary>
        public static void Init( ILogger logger, IHostEnvironment environment )
        {
            
            _logger = logger;
            _applicationName = environment.ApplicationName;
            _environment = environment.EnvironmentName;
        }

        /// <summary>
        /// Add a ILogMessage to the logging queue
        /// </summary>
        /// <param name="message2Log">The ILogMessage that needs to be logged</param>
        /// <returns>true if it was added, false if it was suppressed because of the trace-level</returns>
        public static bool Log(ILogMessage message2Log)
        {
            if (message2Log.Extended == null) message2Log.Extended = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(_applicationName)) message2Log.ApplicationName = _applicationName;
            if (!string.IsNullOrEmpty(_environment)) message2Log.EnvironmentCode = _environment;
            if (!message2Log.Extended.ContainsKey("FullName")) message2Log.Extended.Add("FullName", Assembly.GetExecutingAssembly().FullName);
            if (!message2Log.Extended.ContainsKey("AppDomainName")) message2Log.Extended.Add("AppDomainName", AppDomain.CurrentDomain.FriendlyName);

            switch (message2Log.Level)
            {
                case LogLevel.Error:
                    if (message2Log.Exception != null)
                    {
                        _logger.LogError(message2Log.EventId, message2Log.Message);
                    }
                    else
                    {
                        _logger.LogError(message2Log.Exception,message2Log.Message);
                    }
                    break;
                case LogLevel.Warning:
                    if (message2Log.Exception != null)
                    {
                        _logger.LogWarning(message2Log.EventId, message2Log.Message);
                    }
                    else
                    {
                        _logger.LogWarning(message2Log.Exception, message2Log.Message);
                    }
                    break;
                case LogLevel.Information:
                    if (message2Log.Exception != null)
                    {
                        _logger.LogInformation(message2Log.EventId, message2Log.Message);
                    }
                    else
                    {
                        _logger.LogInformation(message2Log.Exception, message2Log.Message);
                    }
                    break;
                case LogLevel.Debug:
                    if (message2Log.Exception != null)
                    {
                        _logger.LogDebug(message2Log.EventId, message2Log.Message);
                    }
                    else
                    {
                        _logger.LogDebug(message2Log.Exception, message2Log.Message);
                    }
                    break;
                case LogLevel.Trace:
                    if (message2Log.Exception != null)
                    {
                        _logger.LogTrace(message2Log.EventId, message2Log.Message);
                    }
                    else
                    {
                        _logger.LogTrace(message2Log.Exception, message2Log.Message);
                    }
                    break;
                case LogLevel.Critical:
                    if (message2Log.Exception != null)
                    {
                        _logger.LogCritical(message2Log.EventId, message2Log.Message);
                    }
                    else
                    {
                        _logger.LogCritical(message2Log.Exception, message2Log.Message);
                    }
                    break;
            }

            return true;
        }

        public static IDisposable CreateTrace(LoggingBoundaries boundary, params object[] parameterValues)
        {
            if (!_logger.IsEnabled(LogLevel.Trace)) return _dummyTrace;
            return new PerformanceTracer(boundary, parameterValues);
        }


        /// <summary>
        /// Log event for recording error messages
        /// Log event for recording warning messages 
        /// </summary>
        public static bool LogCritical(LoggingBoundaries boundary, params object[] parameterValues)
        {
            if (!_logger.IsEnabled(LogLevel.Critical)) return false;
            return Log(LogMessage.LogCritical(boundary, parameterValues));
        }

        /// <summary>
        /// Log event for recording error messages
        /// Log event for recording warning messages 
        /// </summary>
        public static bool LogError(LoggingBoundaries boundary, params object[] parameterValues)
        {
            if (!_logger.IsEnabled(LogLevel.Error)) return false;
            return Log(LogMessage.LogError(boundary, parameterValues));
        }

        /// <summary>
        /// Log event for recording information messages
        /// </summary>
        public static bool LogInformation(LoggingBoundaries boundary, params object[] parameterValues)
        {
            if (!_logger.IsEnabled(LogLevel.Information)) return false;
            return Log(LogMessage.LogInformation(boundary, parameterValues));
        }

        /// <summary>
        /// Logs string trace message to the log class
        /// </summary>
        public static bool LogTrace(LoggingBoundaries boundary, params object[] parameterValues)
        {
            if (!_logger.IsEnabled(LogLevel.Trace)) return false;
            return Log(LogMessage.LogTrace(boundary, parameterValues));
        }


        /// <summary>
        /// Logs service transmissions
        /// </summary>
        public static bool LogTransmission(string trackingGUID, params object[] parameterValues)
        {
            if (!_logger.IsEnabled(LogLevel.Debug)) return false;
            return Log(LogMessage.LogTransmission(trackingGUID, parameterValues));
        }


        /// <summary>
        /// Log event for recording warning messages 
        /// </summary>
        public static bool LogWarning(LoggingBoundaries boundary, params object[] parameterValues)
        {
            return Log(LogMessage.LogWarning(boundary, parameterValues));
        }

        #endregion Logging Methods

        #region Handle Exceptions

        /// <summary>
        /// Logs exception and determines if exception should be thrown again
        /// </summary>
        /// <param name="boundary">the boundary layer that produced the message</param>
        /// <param name="ex">The exception thrown</param>
        /// <returns>true if the calling method should throw the exception again</returns>
        public static bool HandleException(LoggingBoundaries boundary, Exception ex)
        {
            LogError(boundary, ex);
            return true;
        }

        #endregion Handle Exceptions
    }
}