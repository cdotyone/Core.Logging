#region Copyright / Comments

// <copyright file="LogManager.cs" company="Civic Engineering & IT">Copyright © Civic Engineering & IT 2013</copyright>
// <author>Chris Doty</author>
// <email>dotyc@civicinc.com</email>
// <date>6/4/2013</date>
// <summary></summary>

#endregion Copyright / Comments

using System;

namespace log4net
{
    /// <summary>
    /// this is used for log4net compatiblity
    /// </summary>
    public class LogManager
    {
        #region Methods

        public static ILog GetLogger(Type dummy)
        {
            return new Log4NetWrapper();
        }

        #endregion Methods
    }

    /// <summary>
    /// this is used for log4net compatiblity
    /// </summary>
    public interface ILog
    {
        #region Properties

        bool IsDebugEnabled
        {
            get;
        }

        bool IsErrorEnabled
        {
            get;
        }

        bool IsInfoEnabled
        {
            get;
        }

        #endregion Properties

        #region Methods

        void Debug(string message);

        void Debug(string message, Exception ex);

        void DebugFormat(string message, object ex);

        void DebugFormat(string message);

        void Error(string message);

        void Error(string message, Exception ex);

        void Info(string message);

        #endregion Methods
    }
}