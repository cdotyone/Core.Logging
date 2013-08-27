#region Copyright / Comments

// <copyright file="EventLogger.cs" company="Civic Engineering & IT">Copyright © Civic Engineering & IT 2013</copyright>
// <author>Chris Doty</author>
// <email>dotyc@civicinc.com</email>
// <date>6/4/2013</date>
// <summary></summary>

#endregion Copyright / Comments

#region References

using System;
using System.Collections.Generic;
using System.Diagnostics;

#endregion References

namespace Civic.Core.Logging.LogWriters
{
    [Serializable]
    public class EventLogger : ILogWriter
    {
        #region Fields

        private EventLog _eventlog;

        #endregion Fields

        #region Constructors

        ~EventLogger()
        {
            Shutdown();
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// gets the name given to this log
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
        /// gets the name given to this log
        /// </summary>
        public string LogName { get; private set; }

        /// <summary>
        /// gets the name given to this log
        /// </summary>
        public string Name
        {
            get { return "Event Logger"; }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Creates an event log source
        /// </summary>
        /// <param name="applicationname">Name of the application using this log</param>
        /// <param name="logname">Name of the log, this can be interperted the way the class want to, but it must identify a unique logger.</param>
        public static void CreateEventLog(string applicationname, string logname)
        {
            if (!EventLog.SourceExists(applicationname))
            {
                EventLog.CreateEventSource(applicationname, logname);
            }
        }

        /// <summary>
        /// delete an event log source
        /// </summary>
        /// <param name="applicationname">Name of the application using this log</param>
        /// <param name="logname">name of the log</param>
        public static void DeleteEventLog( string applicationname, string logname )
        {
            if (EventLog.SourceExists(applicationname))
            {
                EventLog.DeleteEventSource(applicationname);
                EventLog.Delete(logname);
            }
        }

        /// <summary>
        /// Initliazes the logger
        /// </summary>
        /// <param name="applicationname">Name of the application using this log</param>
        /// <param name="logname">Name of the log, this can be interperted the way the class want to, but it must identify a unique logger.</param>
        /// <param name="canThread">should the logger us a thread, generally false is suggested for web sites</param>
        /// <param name="addtionalParameters">any additional configuration parameters found on the configuration node for this logger</param>
        public ILogWriter Create( string applicationname, string logname, bool canThread, Dictionary<string, string> addtionalParameters )
        {
            EventLogger ev = new EventLogger();

            ev.LogName = logname;
            ev.ApplicationName = applicationname;

            CreateEventLog(applicationname, logname);

            try
            {
                // create the log object and reference the now defined source
                if (ev._eventlog == null)
                    ev._eventlog = new EventLog();
                ev._eventlog.Source = applicationname;
                ev._eventlog.Log = logname;
            }
            catch (Exception)
            {
                // if the log cannot be initialized null the event object
                ev._eventlog = null;
            }

            return ev;
        }

        /// <summary>
        /// On logs that can be deleted.  
        /// This will delete the log
        /// </summary>
        public void Delete()
        {
            DeleteEventLog(ApplicationName, LogName);
        }

        public void Flush()
        {
        }

        /// <summary>
        /// Logs a message to the log class
        /// </summary>
        /// <param name="message">the message to write the the log</param>
        public bool Log(ILogMessage message)
        {
            switch (message.Type)
            {
                case LogSeverity.Exception:
                    _eventlog.WriteEntry(ApplicationName + " (" + message.Boundary + ")" + " - EXCEPTION: " + message.Message, EventLogEntryType.Error);
                    break;
                case LogSeverity.Error:
                    _eventlog.WriteEntry(ApplicationName + " (" + message.Boundary + ")" + ": " + message.Message, EventLogEntryType.Error);
                    break;
                case LogSeverity.Warning:
                    _eventlog.WriteEntry(ApplicationName + " (" + message.Boundary + ")" + ": " + message.Message, EventLogEntryType.Warning);
                    break;
                case LogSeverity.Information:
                    _eventlog.WriteEntry(ApplicationName + " (" + message.Boundary + ")" + ": " + message.Message, EventLogEntryType.Information);
                    break;
                case LogSeverity.Trace:
                    _eventlog.WriteEntry(ApplicationName + " (" + message.Boundary + ")" + " - TRACE: " + message.Message, EventLogEntryType.Information);
                    return false;
            }
            return true;
        }

        /// <summary>
        /// shuts down and cleans up after logger
        /// </summary>
        public void Shutdown()
        {
            if (_eventlog != null)
            {
                try
                {
                    _eventlog.Close();
                }
                catch (Exception)
                {
                }
                _eventlog = null;
            }
        }

        #endregion Methods
    }
}