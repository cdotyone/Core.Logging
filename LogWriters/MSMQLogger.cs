#region Copyright / Comments

// <copyright file="MSMQLogger.cs" company="Civic Engineering & IT">Copyright © Civic Engineering & IT 2013</copyright>
// <author>Chris Doty</author>
// <email>dotyc@civicinc.com</email>
// <date>6/4/2013</date>
// <summary></summary>

#endregion Copyright / Comments

#region References

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Messaging;

#endregion References

namespace Civic.Core.Logging.LogWriters
{
    [Serializable]
    public class MSMQLogger : ILogWriter
    {
        #region Fields

        private MessageQueue _mqueue;

        #endregion Fields

        #region Constructors

        ~MSMQLogger()
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
            get { return "MSMQ Logger"; }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Initliazes the logger
        /// </summary>
        /// <param name="applicationname">Name of the application using this log</param>
        /// <param name="logname">Name of the log, this can be interperted the way the class want to, but it must identify a unique logger.</param>
        /// <param name="canThread">should the logger use threads.  generally advised this is false for web sites</param>
        /// <param name="addtionalParameters">any additional parameters found on configuration node</param>
        public ILogWriter Create( string applicationname, string logname, bool canThread, Dictionary<string, string> addtionalParameters )
        {
            var ev = new MSMQLogger {LogName = logname, ApplicationName = applicationname};

            try
            {
                // create the log object and reference the now defined source
                if (ev._mqueue == null)
                {
                    if (MessageQueue.Exists(".\\private$\\" + logname))
                    {
                        ev._mqueue = new MessageQueue(".\\private$\\" + logname);
                    }
                    else
                    {
                        ev._mqueue = MessageQueue.Create(".\\private$\\" + logname);
                    }
                }
            }
            catch (Exception ee)
            {
                // if the log cannot be initialized null the event object
                ev._mqueue = null;
                Debug.WriteLine(ee.Message);
            }

            return ev;
        }

        /// <summary>
        /// On logs that can be deleted.  
        /// This will delete the log
        /// </summary>
        public void Delete()
        {
            MessageQueue.Delete(".\\private$\\" + LogName);
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
            try
            {
                _mqueue.Send(message);
            }
            catch
            {
                return false;
            }
            /*
            switch (message.Type)
            {
                case LogSeverity.Error:
                    _mqueue.Send(message);
                    break;
                case LogSeverity.Warning:
                    _mqueue.WriteEntry(_applicationname + ": " + message.Message, EventLogEntryType.Warning);
                    break;
                case LogSeverity.Information:
                    _mqueue.WriteEntry(_applicationname + ": " + message.Message, EventLogEntryType.Information);
                    break;
                case LogSeverity.Trace:
                    return false;
            }*/

            return true;
        }

        /// <summary>
        /// shuts down and cleans up after logger
        /// </summary>
        public void Shutdown()
        {
            if (_mqueue != null)
            {
                try
                {
                    _mqueue.Close();
                }
                catch (Exception)
                {
                }
                _mqueue = null;
            }
        }

        #endregion Methods
    }
}