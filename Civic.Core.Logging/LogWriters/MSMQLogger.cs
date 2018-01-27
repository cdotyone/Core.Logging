using System;
using System.Diagnostics;
using System.IO;
using System.Messaging;
using System.Security;
using System.Text;
using Civic.Core.Logging.Configuration;
using Newtonsoft.Json;

namespace Civic.Core.Logging.LogWriters
{
    [Serializable]
    public class MSMQLogger : ILogWriter, ILogReader
    {
        #region Fields

        private MessageQueue _mqueue;
        private string _path;

        #endregion Fields

        #region Constructors

        ~MSMQLogger()
        {
            Shutdown();
        }

        #endregion Constructors

        #region Properties

        public int HasMessage
        {
            get {
                if (_mqueue == null) return 0;

                try
                {
                    return _mqueue.Peek(TimeSpan.FromMilliseconds(0)) != null ? 1 : 0;
                }
                catch(MessageQueueException ex)
                {
                    if (ex.MessageQueueErrorCode != MessageQueueErrorCode.IOTimeout)
                    {
                        Logger.HandleException(LoggingBoundaries.ServiceBoundary, ex);
                        return -1;
                    }
                    return 0;
                }
            }
        }

        public ILogMessage Receive()
        {
            if (_mqueue == null) return null;

            var msg = _mqueue.Receive();
            if (msg == null) return null;
            var sr = new StreamReader(msg.BodyStream);
            var logMessage = sr.ReadToEnd();

            if (string.IsNullOrEmpty(logMessage)) return null;
            return JsonConvert.DeserializeObject<LogMessage>(logMessage);
        }

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
        public object Create(string applicationname, string logname, LoggerConfig config)
        {
            var ev = new MSMQLogger {LogName = logname, ApplicationName = applicationname};

            var servername = GetMachineName();
            string connectservername;

            var addtionalParameters = config.Attributes;
            if (addtionalParameters != null && addtionalParameters.ContainsKey("serverName"))
                connectservername = addtionalParameters["serverName"];
            else connectservername = servername;

            try
            {
                // create the log object and reference the now defined source

                if (connectservername == servername)
                {
                    ev._path = string.Format("{0}\\private$\\{1}", connectservername, logname);
                    ev._mqueue = MessageQueue.Exists(ev._path) ? new MessageQueue(ev._path) : MessageQueue.Create(ev._path);
                }
                else
                {
                    ev._path = string.Format("FormatName:DIRECT=OS:{0}\\private$\\{1}", connectservername, logname);
                    ev._mqueue = new MessageQueue(ev._path);
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
            if (_mqueue == null) return;
            MessageQueue.Delete(_path);
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
                if (_mqueue == null) return false;

                if (string.IsNullOrEmpty(message.ApplicationName)) message.ApplicationName = ApplicationName;
                var body = JsonConvert.SerializeObject(message);
                var msg = new Message { BodyStream = new MemoryStream(Encoding.ASCII.GetBytes(body)) };
                _mqueue.Send(msg);
            }
            catch
            {
                return false;
            }

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

        public static string GetMachineName()
        {
            string machineName;
            try
            {
                machineName = Environment.MachineName;
            }
            catch (SecurityException)
            {
                machineName = ".";
            }

            return machineName;
        }

        #endregion Methods
    }
}