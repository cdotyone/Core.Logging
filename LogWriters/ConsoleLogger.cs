#region Copyright / Comments

// <copyright file="ConsoleLogger.cs" company="Civic Engineering & IT">Copyright © Civic Engineering & IT 2013</copyright>
// <author>Chris Doty</author>
// <email>dotyc@civicinc.com</email>
// <date>6/4/2013</date>
// <summary></summary>

#endregion Copyright / Comments

#region References

using System;
using System.Collections.Generic;

#endregion References

namespace Civic.Core.Logging.LogWriters
{
    public class ConsoleLogger : ILogWriter
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
            get { return "Console Logger"; }
        }

        #endregion Properties

        #region Methods

        public ILogWriter Create( string applicationname, string logname, bool canThread, Dictionary<string, string> addtionalParameters )
        {
            ConsoleLogger dl = new ConsoleLogger();

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
        }

        public bool Log(ILogMessage message)
        {
            Console.WriteLine( message.Message );
            return true;
        }

        public void Shutdown()
        {
        }

        #endregion Methods
    }
}