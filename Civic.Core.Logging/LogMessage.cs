#region Copyright / Comments

// <copyright file="LogMessage.cs" company="Civic Engineering & IT">Copyright � Civic Engineering & IT 2013</copyright>
// <author>Chris Doty</author>
// <email>dotyc@civicinc.com</email>
// <date>6/4/2013</date>
// <summary></summary>

#endregion Copyright / Comments

#region References

using System;
using System.Collections.Generic;
using System.Security;
using Newtonsoft.Json;

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
            Created = DateTime.UtcNow;
            ServerName = GetMachineName();
        }

        public LogMessage(ILogMessage logMessage)
        {
            Message = logMessage.Message;
            Boundary = logMessage.Boundary;
            Created = logMessage.Created;
            ServerName = logMessage.ServerName;
            ClientCode = logMessage.ClientCode;
            EnvironmentCode = logMessage.EnvironmentCode;
            Extended = logMessage.Extended;
        }

        public LogMessage(LoggingBoundaries boundary, LogSeverity entrytype, params object[] parameterValues) : this()
        {
            Boundary = boundary;
            Type = entrytype;
            var ofs = 0;

            if (parameterValues.Length > 0 && parameterValues[0] is string)
            {
                ofs++;
                Message = parameterValues[0].ToString();
            }
            for (int i = ofs; i < parameterValues.Length; i++)
            {
                if (parameterValues[i] is Dictionary<string,object>)
                {
                    var ext = (Dictionary<string, object>) parameterValues[0];
                    Extended = ext;
                    continue;
                }
                if (parameterValues[i] is Exception)
                {
                    var e = (Exception)parameterValues[i];
                    Message = (string.IsNullOrEmpty(Message) ? string.Empty : Message + "\n") + ExpandException(e);
                    if (parameterValues.Length - 1 > i) Message = "{" + (i + 1) + "}\n" + Message;
                    if(Extended==null) Extended = new Dictionary<string, object>();
                    Extended["StackTrace"] = e.StackTrace;
                    TrackingGUID = e.GetReferenceID().ToString();
                    continue;
                }

                if (string.IsNullOrEmpty(Message)) Message = "{0}";
                if (parameterValues[i].GetType().IsPrimitive || parameterValues[i] is string) Message = Message.Replace("{" + (i - ofs) + "}", parameterValues[i].ToString());
                else Message = Message.Replace("{" + (i - ofs) + "}", JsonConvert.SerializeObject(parameterValues[i]));
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// gets/sets when the message was created
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// gets/sets the extended properties
        /// </summary>
        public Dictionary<string, object> Extended { get; set; }

        /// <summary>
        /// gets/sets the tracking guid for the 
        /// </summary>
        public string TrackingGUID { get; set; }

        /// <summary>
        /// gets/sets the message text for this message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// gets/sets the data packet for this message
        /// </summary>
        public LogSeverity Type { get; set; }

        /// <summary>
        /// gets/sets the name of the client, may be CIVIC for shared services
        /// </summary>
        public string ClientCode { get; set; }

        /// <summary>
        /// gets/sets the name of the environment DEV,TEST,QA,STAGE,PROD
        /// </summary>
        public string EnvironmentCode { get; set; }

        /// <summary>
        /// gets/sets the name of the application
        /// </summary>
        public string ApplicationName { get; set; }

        /// <summary>
        /// gets/sets the name of the application internal identifier
        /// </summary>
        public string ApplicationInternal { get; set; }

        /// <summary>
        /// gets/sets the name of the server
        /// </summary>
        public string ServerName { get; set; }

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
            // ReSharper disable once EmptyGeneralCatchClause
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
            // ReSharper disable once EmptyGeneralCatchClause
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
            // ReSharper disable once EmptyGeneralCatchClause
            catch (Exception)
            {
            }
            return null;
        }


        /// <summary>
        /// Logs service tranmissions
        /// </summary>
        public static LogMessage LogTransmission(string trackingGUID, params object[] parameterValues)
        {
            try
            {
                if (parameterValues.Length == 0) throw new Exception("must provide at least one parameter");
                var message = new LogMessage(LoggingBoundaries.ServiceBoundary, LogSeverity.Trace, parameterValues);
                message.TrackingGUID = trackingGUID;
                return message;
            }
            // ReSharper disable once EmptyGeneralCatchClause
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
            // ReSharper disable once EmptyGeneralCatchClause
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
        public static string ExpandException(Exception ex)
        {
            if ( ex == null ) return "";

            string retval = ex.Message;

            if ( string.IsNullOrEmpty( ex.StackTrace ) ) retval += "\n---" + ex.StackTrace;

            if(ex.InnerException != null)
                retval += "\n\t" + ExpandException( ex.InnerException );

            return retval;
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

        #endregion Methods

    }
}