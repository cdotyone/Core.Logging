#region Copyright / Comments

// <copyright file="ILogMessage.cs" company="Civic Engineering & IT">Copyright � Civic Engineering & IT 2013</copyright>
// <author>Chris Doty</author>
// <email>dotyc@civicinc.com</email>
// <date>6/4/2013</date>
// <summary></summary>

#endregion Copyright / Comments

#region References

using System;
using System.Collections.Generic;

#endregion References

namespace Civic.Core.Logging
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
        string TrackingGUID
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
        LogSeverity Type
        {
            get; set;
        }

        /// <summary>
        /// gets/sets the name of the client, may be CIVIC for shared services
        /// </summary>
        string ClientCode
        {
            get;
            set;
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
        /// gets/sets the name of the application internal identifier
        /// </summary>
        string ApplicationInternal
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

        #endregion Properties
    }
}