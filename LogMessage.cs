#region Copyright / Comments

// <copyright file="LogMessage.cs" company="Civic Engineering & IT">Copyright © Civic Engineering & IT 2013</copyright>
// <author>Chris Doty</author>
// <email>cdoty@polaropposite.com</email>
// <date>6/4/2013</date>
// <summary></summary>

#endregion Copyright / Comments

#region References

using System;
using System.Collections.Generic;

#endregion References

namespace Civic.Core.Logging
{
    [Serializable]
    public class LogMessage : ILogMessage
    {
        #region Constructors

        public LogMessage()
        {
            Message = "";
            Boundary = LoggingBoundaries.Unknown;
        }

        public LogMessage(LoggingBoundaries boundary, LogSeverity entrytype, params object[] parameterValues)
        {
            Boundary = boundary;
            Type = entrytype;

            for (int i = 0; i < parameterValues.Length; i++)
            {
                if (parameterValues[i] is Dictionary<string,object>)
                {
                    var ext = (Dictionary<string, object>) parameterValues[0];
                    Extended = ext;
                    continue;
                }
                if (parameterValues[i] is Exception)
                {
                    var e = (Exception)parameterValues[0];
                    Message = (string.IsNullOrEmpty(Message) ? string.Empty : Message + "\n") + expandException(e);
                    if (parameterValues.Length - 1 > i) Message = "{" + (i + 1) + "}\n" + Message;
                    continue;
                }

                if (string.IsNullOrEmpty(Message)) Message = "{0}";
                Message = Message.Replace("{" + i + "}", parameterValues[i].ToString());
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// gets/sets the extended properties
        /// </summary>
        public Dictionary<string, object> Extended { get; set; }

        /// <summary>
        /// gets/sets the title text for this message
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// gets/sets the message text for this message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// gets/sets the data packet for this message
        /// </summary>
        public LogSeverity Type { get; set; }

        /// <summary>
        /// Layer Boundary
        /// </summary>
        public LoggingBoundaries Boundary { get; set; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Log event for recording error messages
        /// </summary>
        public static LogMessage LogError(LoggingBoundaries boundary, params object[] parameterValues)
        {
            try
            {
                if (parameterValues.Length == 0) throw new Exception("must provide at least one parameter");
                return new LogMessage(boundary, LogSeverity.Error, parameterValues);
            }
            catch (Exception)
            {
            }
            return null;
        }

        /// <summary>
        /// Log event for recording informaiton messages
        /// </summary>
        public static LogMessage LogInformation(LoggingBoundaries boundary, params object[] parameterValues)
        {
            try
            {
                if (parameterValues.Length == 0) throw new Exception("must provide at least one parameter");
                return new LogMessage(boundary, LogSeverity.Information, parameterValues);
            }
            catch (Exception)
            {
            }
            return null;
        }

        /// <summary>
        /// Logs string trace message to the log class
        /// </summary>
        public static LogMessage LogTrace(LoggingBoundaries boundary, params object[] parameterValues)
        {
            try
            {
                if (parameterValues.Length == 0) throw new Exception("must provide at least one parameter");
                return new LogMessage(boundary, LogSeverity.Trace, parameterValues);
            }
            catch (Exception)
            {
            }
            return null;
        }

        /// <summary>
        /// Log event for recording warning messages 
        /// </summary>
        public static LogMessage LogWarning(LoggingBoundaries boundary, params object[] parameterValues)
        {
            try
            {
                if (parameterValues.Length == 0) throw new Exception("must provide at least one parameter");
                return new LogMessage(boundary, LogSeverity.Warning, parameterValues);
            }
            catch (Exception)
            {
            }
            return null;
        }

        /// <summary>
        /// expands an exception to a string that can be turned into a log entry.
        /// </summary>
        /// <param name="ex">the exception to be expanded</param>
        /// <returns>the expanded exception in string form</returns>
        private static string expandException(Exception ex)
        {
            if ( ex == null ) return "";

            string retval = ex.Message;

            if ( string.IsNullOrEmpty( ex.StackTrace ) ) retval += "\n---" + ex.StackTrace;

            if(ex.InnerException != null)
                retval += "\n\t" + expandException( ex.InnerException );

            return retval;
        }

        #endregion Methods
    }
}