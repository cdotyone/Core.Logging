#region References

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

#endregion References

namespace Core.Logging
{
    #region Enumerations

    #endregion Enumerations

    /// <summary>
    /// Describes a generic log message class
    /// </summary>
    public interface ILogMessage
    {
        #region Properties

        /// <summary>
        /// gets/sets the tracking guid for the 
        /// </summary>
        string EventId
        {
            get; set;
        }

        /// <summary>
        /// gets/sets the message text for this message
        /// </summary>
        string Message
        {
            get; set;
        }

        /// <summary>
        /// gets/sets the message text for this message
        /// </summary>
        LogLevel Level
        {
            get; set;
        }

        /// <summary>
        /// gets/sets the name of the environment DEV,TEST,QA,STAGE,PROD
        /// </summary>
        string EnvironmentCode
        {
            get;
            set;
        }

        /// <summary>
        /// gets/sets the name of the application
        /// </summary>
        string ApplicationName
        {
            get;
            set;
        }

        /// <summary>
        /// gets/sets the name of the server
        /// </summary>
        string ServerName
        {
            get;
            set;
        }

        /// <summary>
        /// gets/sets when the message was created
        /// </summary>
        DateTime Created
        {
            get;
            set;
        }

        /// <summary>
        /// gets/sets the extended properties
        /// </summary>
        Dictionary<string, object> Extended { get; set; }

        /// <summary>
        /// gets/sets the Layer Boundary
        /// </summary>
        LoggingBoundaries Boundary { get; set; }

        /// <summary>
        /// Exception if there is one
        /// </summary>
        Exception Exception { get; set; }

        #endregion Properties
    }
}