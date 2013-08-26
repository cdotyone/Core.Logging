#region Copyright / Comments

// <copyright file="WebCommentLogger.cs" company="Civic Engineering & IT">Copyright © Civic Engineering & IT 2013</copyright>
// <author>Chris Doty</author>
// <email>cdoty@polaropposite.com</email>
// <date>6/4/2013</date>
// <summary></summary>

#endregion Copyright / Comments

#region References

using System;
using System.Collections.Generic;

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
        /// Initliazes the logger
        /// </summary>
        /// <param name="applicationname">Name of the application using this log</param>
        /// <param name="logname">Name of the log, this can be interperted the way the class want to, but it must identify a unique logger.</param>
        /// <param name="canThread">should the logger us a thread, generally false is suggested for web sites</param>
        /// <param name="addtionalParameters">any additional configuration parameters found on the configuration node for this logger</param>
        public ILogWriter Create( string applicationname, string logname, bool canThread, Dictionary<string, string> addtionalParameters )
        {
            WebCommentLogger ev = new WebCommentLogger();

            ev._logname = logname;
            ev._applicationname = applicationname;

            return ev;
        }

        /// <summary>
        /// On logs that can be deleted.  
        /// This will delete the log
        /// </summary>
        public void Delete()
        {
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
                if ( System.Web.HttpContext.Current != null )
                {
                    System.Web.HttpContext.Current.Response.Write( "<!--\r\n\ttitle: " + message.Title + "\r\n\tmessage: " + message.Message.Replace( "<", "&lt;" ).Replace( ">", "&gt;" ) + "\r\n-->\r\n" );
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