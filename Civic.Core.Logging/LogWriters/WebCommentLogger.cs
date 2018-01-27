#region Copyright / Comments

// <copyright file="WebCommentLogger.cs" company="Civic Engineering & IT">Copyright © Civic Engineering & IT 2013</copyright>
// <author>Chris Doty</author>
// <email>dotyc@civicinc.com</email>
// <date>6/4/2013</date>
// <summary></summary>

#endregion Copyright / Comments

#region References

using System;
using System.Collections.Generic;
using System.Globalization;
using Civic.Core.Logging.Configuration;

#endregion References

namespace Civic.Core.Logging.LogWriters
{
    [Serializable]
    public class WebCommentLogger : ILogWriter
    {
        #region Fields

        private string _applicationname;
        private string _logname;

        #endregion Fields

        #region Constructors

        ~WebCommentLogger()
        {
            Shutdown();
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// gets the name given to this log
        /// </summary>
        public string ApplicationName
        {
            get { return _applicationname; }
        }

        /// <summary>
        /// true if the ILogWriter supports a delete command
        /// false if it does not
        /// </summary>
        public bool CanDelete
        {
            get { return false; }
        }

        /// <summary>
        /// gets the name given to this log
        /// </summary>
        public string LogName
        {
            get { return _logname; }
        }

        /// <summary>
        /// gets the name given to this log
        /// </summary>
        public string Name
        {
            get { return "WebCommentLogger Logger"; }
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
            var ev = new WebCommentLogger
                {
                    _logname = logname, 
                    _applicationname = applicationname
                };

            return ev;
        }

        /// <summary>
        /// Does nothing, required by interface
        /// </summary>
        public void Delete()
        {
        }

        /// <summary>
        /// Does nothing, required by interface
        /// </summary>
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
                if (string.IsNullOrEmpty(message.ApplicationName)) message.ApplicationName = ApplicationName;

                if ( System.Web.HttpContext.Current != null )
                {
                    System.Web.HttpContext.Current.Response.Write("<!--\r\n\tcreated: " + message.Created.ToString(CultureInfo.InvariantCulture) + "\r\n\tmessage: " + message.Message.Replace("<", "&lt;").Replace(">", "&gt;") + "\r\n-->\r\n");
                }
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
        }

        #endregion Methods
    }
}