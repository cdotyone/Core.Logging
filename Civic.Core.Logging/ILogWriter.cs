#region Copyright / Comments

// <copyright file="ILogWriter.cs" company="Civic Engineering & IT">Copyright © Civic Engineering & IT 2013</copyright>
// <author>Chris Doty</author>
// <email>dotyc@civicinc.com</email>
// <date>6/4/2013</date>
// <summary></summary>

#endregion Copyright / Comments

#region References

using System.Collections.Generic;

#endregion References

namespace Civic.Core.Logging
{
    /// <summary>
    /// Describes a log writer
    /// </summary>
    public interface ILogWriter
    {
        #region Properties

        /// <summary>
        /// gets the application name given to this logger
        /// </summary>
        string ApplicationName
        {
            get;
        }

        /// <summary>
        /// true if the ILogWriter supports a delete command
        /// false if it does not
        /// </summary>
        bool CanDelete
        {
            get;
        }

        /// <summary>
        /// gets the log name given to this log
        /// </summary>
        string LogName
        {
            get;
        }

        /// <summary>
        /// gets the display name given to this logger
        /// </summary>
        string Name
        {
            get;
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Used by factory to create objects of this type
        /// </summary>
        /// <param name="applicationname">application name given to this logger</param>
        /// <param name="logname">log name given to this log</param>
        /// <param name="canThread">tells the log write if it can use threads</param>
        /// <param name="addtionalParameters">addtional attributes from the configuration of this logger</param>
        /// <returns></returns>
        object Create(string applicationname, string logname, bool canThread, Dictionary<string, string> addtionalParameters);

        /// <summary>
        /// On logs that can be deleted.  
        /// This will delete the log
        /// </summary>
        void Delete();

        /// <summary>
        /// forces all items left in logger queue to write out to it's storage device
        /// </summary>
        void Flush();

        /// <summary>
        /// Logs a message to the log class
        /// </summary>
        /// <param name="message">the message to write the the log</param>
        bool Log(ILogMessage message);

        /// <summary>
        /// shuts down and cleans up after logger
        /// </summary>
        void Shutdown();

        #endregion Methods
    }
}