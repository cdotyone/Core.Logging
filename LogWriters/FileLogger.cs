#region Copyright / Comments

// <copyright file="FileLogger.cs" company="Civic Engineering & IT">Copyright © Civic Engineering & IT 2013</copyright>
// <author>Chris Doty</author>
// <email>dotyc@civicinc.com</email>
// <date>6/4/2013</date>
// <summary></summary>

#endregion Copyright / Comments

#region References

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

#endregion References

namespace Civic.Core.Logging.LogWriters
{
    [Serializable]
    public class FileLogger : ILogWriter
    {
        #region Fields

        private string _lastlogdate;
        private AutoResetEvent _logWaiting;
        private string _logfilename;
        private bool _running;
        private AutoResetEvent _shutDown;
        [NonSerialized]
        private Thread _tm;
        private Queue<ILogMessage> _eventQueue;

        #endregion Fields

        #region Constructors

        public FileLogger()
        {
            _logWaiting = new AutoResetEvent(false);
            _shutDown = new AutoResetEvent(false);
            _eventQueue = new Queue<ILogMessage>();

            LogFileFormat = "<NAME>yyyyMMdd.log";
            _lastlogdate = "";
        }

        ~FileLogger()
        {
            Shutdown();
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// gets the name given to this log
        /// </summary>
        public string ApplicationName { get; set; }

        /// <summary>
        /// true if the ILogWriter supports a delete command
        /// false if it does not
        /// </summary>
        public bool CanDelete
        {
            get { return false; }
        }

        /// <summary>
        /// get/sets log file format for log files
        /// </summary>
        public string LogFileFormat { get; set; }

        /// <summary>
        /// get/sets log file path for log files
        /// </summary>
        public string LogFilePath { get; set; }

        /// <summary>
        /// gets the name given to this log
        /// </summary>
        public string LogName { get; set; }

        /// <summary>
        /// gets the name given to this log
        /// </summary>
        public string Name
        {
            get { return "File Logger"; }
        }

        /// <summary>
        /// gets current log entries left to process
        /// </summary>
        protected int PendingLogs
        {
            get { return _eventQueue.Count; }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// clears any remaining log entries left to process
        /// </summary>
        public void Clear()
        {
            // Lock for writing
            lock (_eventQueue)
            {
                _eventQueue.Clear();
            }
        }

        /// <summary>
        /// Initliazes the logger
        /// </summary>
        /// <param name="applicationname">Name of the application using this log</param>
        /// <param name="logname">Name of the log, this can be interperted the way the class want to, but it must identify a unique logger.</param>
        /// <param name="canThread">should the logger us a thread, generally false is suggested for web sites</param>
        /// <param name="addtionalParameters">any additional configuration parameters found on the configuration node for this logger</param>
        public ILogWriter Create(string applicationname, string logname, bool canThread, Dictionary<string, string> addtionalParameters)
        {
            var fl = new FileLogger();

            fl.ApplicationName = applicationname;
            fl.LogFileFormat = "<NAME>yyyyMMdd.log";
            fl._lastlogdate = "";

            fl.LogName = logname;
            fl._eventQueue = new Queue<ILogMessage>();

            if ( canThread )
            {
                fl._tm = new Thread( Process );
                fl._tm.Name = "File Logger Process";
                fl._tm.Start();
            }
            else fl._tm = null;

            try
            {
                if (!string.IsNullOrEmpty(addtionalParameters["logpath"]))
                    fl.LogFilePath = addtionalParameters["logpath"];
            }
            catch { }

            try
            {
                if (!string.IsNullOrEmpty(addtionalParameters["logfileformat"]))
                    fl.LogFileFormat = addtionalParameters["logfileformat"];
            }
            catch { }

            return fl;
        }

        /// <summary>
        /// On logs that can be deleted.  
        /// This will delete the log
        /// </summary>
        public void Delete()
        {
            throw new Exception("The method or operation is not supported.");
        }

        public void Flush()
        {
            RunQueue();
        }

        /// <summary>
        /// Logs a message to the log class
        /// </summary>
        /// <param name="message">the message to write the the log</param>
        public bool Log(ILogMessage message)
        {
            // Lock for writing
            lock (_eventQueue)
            {
                _eventQueue.Enqueue(message);
            }

            if (!_running) RunQueue();
            else _logWaiting.Set();
            return true;
        }

        /// <summary>
        /// shuts down and cleans up after logger
        /// </summary>
        public void Shutdown()
        {
            _running = false;

            if ( _tm != null )
            {
                // Set shutdown flag
                _shutDown.Set();
                _logWaiting.Set();
                Thread.Sleep( 100 );

                // Double check there are no more events on queue, if so, run them.
                if ( PendingLogs > 0 )
                {
                    RunQueue();
                }

                try
                {
                    _tm.Abort();
                    _tm = null;
                }
                catch
                {
                    // Do nothing...
                }
            }
        }

        /// <summary>
        /// Process the queue on a thread
        /// </summary>
        protected void Process()
        {
            _running = true;
            // Loop and monitor every second.
            while (_logWaiting.WaitOne())
            {
                if (_shutDown.WaitOne(0, true))
                    break;
                try
                {
                    RunQueue();
                }
                catch
                {
                    // Catch everything and do nothing
                }
                Thread.Sleep(100); // slow us down
            }
        }

        /// <summary>
        /// Run any events currently on queue
        /// </summary>
        protected void RunQueue()
        {
            TextWriter output = null;
            if (PendingLogs > 0)
            {
                DateTime dt = DateTime.Now;

                if (_lastlogdate != dt.ToShortDateString())
                {
                    // get the file extension
                    string[] parts = LogFileFormat.Split('.');
                    _logfilename = ".log";
                    if (parts.Length > 1)
                        _logfilename = "." + parts[1];

                    // get the date format for the filename
                    parts = parts[0].Split('>');
                    if (parts[0].IndexOf("<") > -1 && parts.Length > 1)
                        _logfilename = parts[0].Replace("<NAME", LogName) + dt.ToString(parts[1]) + _logfilename;
                    else
                        if (parts[1].IndexOf("<") > -1 && parts.Length > 1)
                            _logfilename = dt.ToString(parts[0]) + parts[1].Replace("<NAME", LogName) + _logfilename;
                        else
                            if (parts[0].IndexOf("<") > -1)
                                _logfilename = parts[1].Replace("<NAME", LogName) + _logfilename;
                            else
                                if (parts[0].IndexOf("<") == 1 && parts[0].IndexOf(">") == 1)
                                    _logfilename = LogName + _logfilename;

                    // add the file path
                    if (LogFilePath == null) LogFilePath = "";
                    if (LogFilePath.Length > 0)
                    {
                        if (LogFilePath[LogFilePath.Length - 1] != '\\')
                            _logfilename = LogFilePath + "\\" + _logfilename;
                        else
                            _logfilename = LogFilePath + _logfilename;
                    }

                    _lastlogdate = dt.ToShortDateString();
                }

                // Open the logfile
                int count = 0;
                while (count<6) {
                    try {
                        count++;
                        output = File.AppendText(_logfilename);
                        break;
                    } catch {
                        Thread.Sleep( 500 );
                    }
                }
            }

            // Loop until queue is empty
            while (PendingLogs > 0)
            {
                LogMessage m;

                // Lock for writing
                lock (_eventQueue)
                {
                    m = (LogMessage)_eventQueue.Dequeue();
                }

                // Output log line
                if (m != null && output!=null)
                {
                    DateTime t = DateTime.Now;

                    // stop loggin in this file if we have rolled into a new day
                    if (t.ToShortDateString() != _lastlogdate)
                        break;

                    switch (m.Type)
                    {
                        case LogSeverity.Error:
                            output.WriteLine("[{0}] {1} - {2}", t, "ERROR:", m.Message);
                            break;
                        case LogSeverity.Warning:
                            output.WriteLine("[{0}] {1} - {2}", t, "WARNING:", m.Message);
                            break;
                        case LogSeverity.Information:
                            output.WriteLine("[{0}] {1} - {2}", t, "INFO:", m.Message);
                            break;
                        case LogSeverity.Trace:
                            output.WriteLine("[{0}] {1} - {2}", t, "TRACE:", m.Message);
                            break;
                    }
                }
            }

            // Close the log file
            if(output!=null) output.Close();
        }

        #endregion Methods
    }
}