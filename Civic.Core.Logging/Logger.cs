#region References

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Reflection;
using System.Security;
using System.Security.Principal;
using System.Threading;
using System.Xml;
using Civic.Core.Configuration;
using Civic.Core.Logging.Configuration;

#endregion References

namespace Civic.Core.Logging
{
    /// <summary>
    /// Primary logging class.  This does all of the routing to the log writers
    /// </summary>
    public class Logger : IConfigurationSectionHandler
    {
        #region Fields

        //private static readonly Dictionary<string, ILogWriter> _availLoggers = new Dictionary<string, ILogWriter>();
        private static readonly Queue<ILogMessage> _eventQueue = new Queue<ILogMessage>();
        private static readonly Dictionary<string, bool> _policies = new Dictionary<string, bool>();

        private static LoggingConfig _config;
        private static Thread _tm;
        private static readonly IDisposable _dummyTrace = new PerformanceTracerDummy();
        private static readonly object _lock = new object();
        private static List<ILogWriter> _loggers = new List<ILogWriter>();
        private static bool _isShutdown = true;

        #endregion Fields

        #region Properties

        /// <summary>
        /// True if the logging thread is shutdown, false if it is running
        /// </summary>
        public static bool IsShutdown
        {
            get { return _isShutdown; }
            private set { _isShutdown = value; }
        }

        /// <summary>
        /// Default trace level for all of the loggers
        /// If set to -1 the defaults for the loggers are used
        /// The higher the number the more detail that is provided
        /// 
        /// this property should be from -1 to 5
        /// </summary>
        public static bool IsTraceOn { get; set; }

        /// <summary>
        /// The current log writers that are installed
        /// </summary>
        public static List<ILogWriter> Loggers
        {
            get { return _loggers; }
            private set { _loggers = value; }
        }

        /// <summary>
        /// gets current log entries left to process
        /// </summary>
        protected static int PendingLogEntries
        {
            get { return _eventQueue.Count; }
        }

        #endregion Properties

        #region Logging Methods

        /// <summary>
        /// clears any remaining log entries left to process
        /// </summary>
        public static void Clear()
        {
            // Lock for writing
            lock (_eventQueue)
            {
                _eventQueue.Clear();
            }
        }

        /// <summary>
        /// Calls delete method on all sub
        /// </summary>
        public static void DeleteAll()
        {
            foreach (ILogWriter logger in Loggers)
            {
                if (logger.CanDelete)
                    logger.Delete();
            }
        }

        /// <summary>
        /// Process the queue on a thread
        /// </summary>
        public static void Flush()
        {
            RunQueue();

            try
            {
                foreach (ILogWriter iLog in Loggers)
                {
                    iLog.Flush();
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Initializes the logger library
        /// Normally not called by consumming application, but automatically called
        /// when a log entry is requested.  Can be called to prime the log system
        /// before it is used.
        /// </summary>
        public static void Init()
        {
            if (!IsShutdown) return;

            lock (_lock)
            {
                if (_config == null) _config = LoggingConfig.Current;
                else return;

                if (_loggers == null) _loggers = new List<ILogWriter>();
                _loggers.Clear();
            
                if (_config != null)
                {
                    IsTraceOn = _config.Trace;

                    // load the loggers
                    foreach (LoggerConfig logger in _config.Loggers)
                    {
                        var logwriter = DynamicInstance.CreateInstance<ILogWriter>(logger.Assembly, logger.Type);
                        var obj = logwriter.Create(_config.ApplicationName, _config.LogName, _config.UseThread, logger.Attributes);
                        _loggers.Add((ILogWriter)obj);
                    }
                
                    foreach (ExceptionPolicyElement policy in _config.ExceptionPolicies)
                    {
                        string key = !string.IsNullOrEmpty(policy.Type) ? policy.Type : policy.Boundary.ToString();
                        if (!_policies.ContainsKey(key)) _policies.Add(key, policy.Rethrow);
                    }
                
                    if (_config.UseThread)
                    {
                        IsShutdown = false;
                        _tm = new Thread(Process) { Name = "Multi Logger Process" };
                        _tm.Start();
                    }
                    else _tm = null;
                }
            }
        }

        /// <summary>
        /// Add a ILogMessage to the loggin queue
        /// </summary>
        /// <param name="message2Log">The ILogMessage that needs to be logged</param>
        /// <returns>true if it was added, false if it was suppressed because of the tracelevel</returns>
        public static bool Log(ILogMessage message2Log)
        {
            Init();

            if (message2Log.Type == LogSeverity.Trace)
            {
                if (!IsTraceOn)
                {
                    return false;
                }
            }
            
            if (message2Log.Extended == null) message2Log.Extended = new Dictionary<string, object>();
            if (_config != null && string.IsNullOrEmpty(message2Log.ApplicationName)) message2Log.ApplicationName = _config.ApplicationName;
            if (_config != null && string.IsNullOrEmpty(message2Log.ClientCode)) message2Log.ClientCode = _config.ClientCode;
            if (_config != null && string.IsNullOrEmpty(message2Log.EnvironmentCode)) message2Log.EnvironmentCode = _config.EnvironmentCode;
            if (!message2Log.Extended.ContainsKey("FullName")) message2Log.Extended.Add("FullName", Assembly.GetExecutingAssembly().FullName);
            if (!message2Log.Extended.ContainsKey("AppDomainName")) message2Log.Extended.Add("AppDomainName", AppDomain.CurrentDomain.FriendlyName);
            if (!message2Log.Extended.ContainsKey("ThreadIdentity")) message2Log.Extended.Add("ThreadIdentity", Thread.CurrentPrincipal.Identity.Name);
            if (!message2Log.Extended.ContainsKey("WindowsIdentity")) message2Log.Extended.Add("WindowsIdentity", GetWindowsIdentity());

            lock (_eventQueue)
            {
                _eventQueue.Enqueue(message2Log);
            }

            if (IsShutdown) Flush();
            return true;
        }


        public static string GetMachineName()
        {
            string machineName;
            try
            {
                machineName = Environment.MachineName;
            }
            catch (SecurityException)
            {
                machineName = "Permission Denied";
            }

            return machineName;
        }

        public static string GetWindowsIdentity()
        {
            string windowsIdentity;
            try
            {
                var current = WindowsIdentity.GetCurrent();
                windowsIdentity = (current == null) ? string.Empty : current.Name;
            }
            catch (SecurityException)
            {
                windowsIdentity = "Permission Denied";
            }

            return windowsIdentity;
        }


        /// <summary>
        /// Log event for recording error messages
        /// </summary>
        public static bool LogError(LoggingBoundaries boundary, params object[] parameterValues)
        {
            return Log(LogMessage.LogError(boundary, parameterValues));
        }

        /// <summary>
        /// Log event for recording informaiton messages
        /// </summary>
        public static bool LogInformation(LoggingBoundaries boundary, params object[] parameterValues)
        {
            return Log(LogMessage.LogInformation(boundary, parameterValues));
        }

        /// <summary>
        /// Logs string trace message to the log class
        /// </summary>
        public static bool LogTrace(LoggingBoundaries boundary, params object[] parameterValues)
        {
            if (_config == null) Init();
            if (!IsTraceOn) return false;
            return Log(LogMessage.LogTrace(boundary, parameterValues));
        }

        public static IDisposable CreateTrace(LoggingBoundaries boundary, params object[] parameterValues)
        {
            if (_config == null) Init();
            if (!IsTraceOn) return _dummyTrace;
            return new PerformanceTracer(boundary, parameterValues);
        }

        /// <summary>
        /// Log event for recording warning messages 
        /// </summary>
        public static bool LogWarning(LoggingBoundaries boundary, params object[] parameterValues)
        {
            return Log(LogMessage.LogWarning(boundary, parameterValues));
        }

        /// <summary>
        /// shuts down and cleans up after logger
        /// </summary>
        public static void Shutdown()
        {
            // Set shutdown flag
            IsShutdown = true;
            Thread.Sleep(100);

            // Sleep a little longer, wait for the thread to finish
            if (_config != null && _config.UseThread) Thread.Sleep(2000);

            // Double check there are no more events on queue, if so, run them.
            if (PendingLogEntries > 0)
            {
                RunQueue();
            }

            try
            {
                if (_config != null && _config.UseThread)
                {
                    _tm.Abort();
                    _tm = null;
                }
            }
            catch (Exception)
            {
                // Do nothing...
            }

            Loggers.Clear();
        }

        /// <summary>
        /// handles the configuration for this library
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="configContext"></param>
        /// <param name="section"></param>
        /// <returns>a logger with a config</returns>
        public object Create(object parent, object configContext, XmlNode section)
        {
            return new Logger();
        }

        /// <summary>
        /// Process the queue on a thread
        /// </summary>
        protected static void Process()
        {
            // Loop and monitor every second.
            while (!IsShutdown)
            {
                try
                {
                    RunQueue();
                }
                catch (Exception)
                {
                    // Catch everything and do nothing
                }

                // If we have more to process lets rest for short time, if not,
                // lets take a longer rest...
                Thread.Sleep(PendingLogEntries > 0 ? 10 : 1000);
            }
        }

        /// <summary>
        /// Run any events currently on queue
        /// </summary>
        protected static void RunQueue()
        {
            // Loop until queue is empty
            while (PendingLogEntries > 0)
            {
                LogMessage m = null;

                // Lock for writing
                lock (_eventQueue)
                {
                    if (PendingLogEntries > 0)
                        m = (LogMessage)_eventQueue.Dequeue();
                }

                // Output log line
                if (m != null)
                {
                    try
                    {
                        foreach (ILogWriter iLog in Loggers)
                        {
                            iLog.Log(m);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
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
            Init();

            LogError(boundary, ex);

            var key = boundary + "_" + ex.GetType().FullName;
            if (_policies.ContainsKey(key)) return _policies[key];
            key = boundary.ToString();
            if (_policies.ContainsKey(key)) return _policies[key];
            return true;
        }

        #endregion Handle Exceptions
    }
}