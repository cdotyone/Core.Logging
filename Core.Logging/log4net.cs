using System;
using log4net;

namespace Core.Logging
{
    /// <summary>
    /// this is used for log4net compatibility
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
    /// this is used for log4net compatibility
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

        void Warn(string message);

        #endregion Methods
    }
}