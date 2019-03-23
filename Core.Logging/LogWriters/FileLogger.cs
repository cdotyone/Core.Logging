#region Copyright / Comments

// <copyright file="FileLogger.cs" company="Civic Engineering & IT">Copyright © Civic Engineering & IT 2013</copyright>
// <author>Chris Doty</author>
// <email>dotyc@civicinc.com</email>
// <date>6/4/2013</date>
// <summary></summary>

#endregion Copyright / Comments

#region References

using System;
using System.IO;
using System.Threading.Tasks;
using Core.Logging.Configuration;
using Newtonsoft.Json;

#endregion References

namespace Core.Logging.LogWriters
{
    [Serializable]
    public class FileLogger : ILogWriter
    {
        #region Fields

        private string _lastlogdate;
        private string _logfilename;

        #endregion Fields

        #region Constructors

        public FileLogger()
        {
            LogFileFormat = "<NAME>yyyyMMdd.log";
            _lastlogdate = "";
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
            var fl = new FileLogger
                {
                    ApplicationName = applicationname,
                    LogFileFormat = "<NAME>yyyyMMdd.log",
                    _lastlogdate = "",
                    LogName = logname,
                };

            if (config.Attributes.ContainsKey("logpath") && !string.IsNullOrEmpty(config.Attributes["logpath"]))
                fl.LogFilePath = config.Attributes["logpath"];
            if (config.Attributes.ContainsKey("logfileformat") && !string.IsNullOrEmpty(config.Attributes["logfileformat"]))
                fl.LogFileFormat = config.Attributes["logfileformat"];

            return fl;
        }

        /// <summary>
        /// On logs that can be deleted.  
        /// This will delete the log
        /// </summary>
        public void Delete()
        {
            //throw new Exception("The method or operation is not supported.");
        }

        /// <summary>
        /// forces all items left in logger queue to write out to it's storage device
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

                try
                {
                    AppendToLog(message);
                }
                catch (Exception ex)
                {
                    Logger.HandleException(LoggingBoundaries.Unknown, ex);
                    return new LogWriterResult { Success = false, Name = Name, Message = message};
                }

                return new LogWriterResult {Success = true, Name = Name};;
            });
        }

        /// <summary>
        /// Run any events currently on queue
        /// </summary>
        protected void AppendToLog(ILogMessage message)
        {
            TextWriter output = null;
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
                else if (parts[1].IndexOf("<") > -1 && parts.Length > 1)
                    _logfilename = dt.ToString(parts[0]) + parts[1].Replace("<NAME", LogName) + _logfilename;
                else if (parts[0].IndexOf("<") > -1)
                    _logfilename = parts[1].Replace("<NAME", LogName) + _logfilename;
                else if (parts[0].IndexOf("<") == 1 && parts[0].IndexOf(">") == 1)
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


            using (output = File.AppendText(_logfilename))
            {
                // Output log line
                if (message != null)
                {
                    DateTime t = DateTime.Now;

                    switch (message.Type)
                    {
                        case LogSeverity.Exception:
                            output.WriteLine("[{0}] {1} - {2}", t, "EXCEPTION:", message.Message);
                            writeExtended(output, message);
                            break;
                        case LogSeverity.Error:
                            output.WriteLine("[{0}] {1} - {2}", t, "ERROR:", message.Message);
                            writeExtended(output, message);
                            break;
                        case LogSeverity.Warning:
                            output.WriteLine("[{0}] {1} - {2}", t, "WARNING:", message.Message);
                            writeExtended(output, message);
                            break;
                        case LogSeverity.Information:
                            output.WriteLine("[{0}] {1} - {2}", t, "INFO:", message.Message);
                            writeExtended(output, message);
                            break;
                        case LogSeverity.Trace:
                            output.WriteLine("[{0}] {1} - {2}", t, "TRACE:", message.Message);
                            writeExtended(output, message);
                            break;
                    }
                }
            }
        }

        private void writeExtended(TextWriter output,ILogMessage message)
        {
            if (message.Extended != null && message.Extended.Count > 0)
            {
                output.WriteLine("\t{0}", JsonConvert.SerializeObject(message.Extended));
            }
        }

        #endregion Methods
    }
}