using System.Collections.Generic;

namespace Civic.Core.Logging
{
    /// <summary>
    /// Describes a log reader
    /// </summary>
    public interface ILogReader
    {
        int HasMessage { get; }

        ILogMessage Receive();

        /// <summary>
        /// Used by factory to create objects of this type
        /// </summary>
        /// <param name="applicationname">application name given to this logger</param>
        /// <param name="logname">log name given to this log</param>
        /// <param name="canThread">tells the log write if it can use threads</param>
        /// <param name="useFailureRecovery">Not used, compatibility for ILogWriter</param>
        /// <param name="addtionalParameters">addtional attributes from the configuration of this logger</param>
        /// <returns></returns>
        object Create(string applicationname, string logname, bool canThread, bool useFailureRecovery, Dictionary<string, string> addtionalParameters);
    }
}