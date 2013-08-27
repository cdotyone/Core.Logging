#region Copyright / Comments

// <copyright file="DebugLogger.cs" company="Civic Engineering & IT">Copyright © Civic Engineering & IT 2013</copyright>
// <author>Chris Doty</author>
// <email>dotyc@civicinc.com</email>
// <date>6/4/2013</date>
// <summary></summary>

#endregion Copyright / Comments

#region References

using System;
using System.Collections.Generic;
using System.Diagnostics;

#endregion References

namespace Civic.Core.Logging.LogWriters
{
    public class DebugLogger : ILogWriter
    {
        #region Properties

        public string ApplicationName { get; private set; }

        public bool CanDelete
        {
            get { return true; }
        }

        public string LogName { get; private set; }

        public string Name
        {
            get { return "Debug Logger"; }
        }

        #endregion Properties

        #region Methods

        public ILogWriter Create( string applicationname, string logname, bool canThread, Dictionary<string, string> addtionalParameters )
        {
            DebugLogger dl = new DebugLogger();

            dl.ApplicationName = applicationname;
            dl.LogName = logname;

            return dl;
        }

        public void Delete()
        {
            throw new Exception("The method or operation is not supported.");
        }

        public void Flush()
        {
            Debug.Flush();
        }

        public bool Log(ILogMessage message)
        {
            Debug.AutoFlush = true;

            switch (message.Type)
            {
                case LogSeverity.Exception:
                    Debug.WriteLine(ApplicationName + " (" + message.Boundary + ") - EXCEPTION: " + message.Message);
                    break;
                case LogSeverity.Error:
                    Debug.WriteLine(ApplicationName + " (" + message.Boundary + ") - ERROR: " + message.Message);
                    break;
                case LogSeverity.Warning:
                    Debug.WriteLine(ApplicationName + " (" + message.Boundary + ") - WARNING: " + message.Message);
                    break;
                case LogSeverity.Information:
                    Debug.WriteLine(ApplicationName + " (" + message.Boundary + ") - INFORMATION: " + message.Message);
                    break;
                case LogSeverity.Trace:
                    Debug.WriteLine(ApplicationName + " (" + message.Boundary + ")" + " - TRACE: " + message.Message);
                    return false;
            }

            return true;
        }

        public void Shutdown()
        {
        }

        #endregion Methods
    }
}