using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Civic.Core.Configuration;
using Civic.Core.Logging.LogWriters;

namespace Civic.Core.Logging.Configuration {

    public class LoggerSection : Section
    {

        private static LoggerSection _coreConfig;

        /// <summary>
        /// The current configuration for caching module
        /// </summary>
        public static LoggerSection Current
        {
            get
            {
                // load the configuration from the config file
                string configNode = ConfigurationManager.AppSettings["LoggerConfigNode"];
                if (string.IsNullOrEmpty(configNode)) configNode = Constants.CORE_LOGGING_SECTION;

                if (_coreConfig == null) _coreConfig = (LoggerSection) ConfigurationManager.GetSection(configNode);


                return _coreConfig ?? (_coreConfig = new LoggerSection());
            }
        }

        [ConfigurationProperty(Constants.CONFIG_APP_PROP, IsKey = false, IsRequired = true)]
        public string App
        {
            get
            {
                if (string.IsNullOrEmpty((string)base[Constants.CONFIG_APP_PROP]))
                {
                    if (string.IsNullOrEmpty(ConfigurationManager.AppSettings[Constants.CONFIG_APPNAME_PROP])) return "Unknown";
                    return ConfigurationManager.AppSettings[Constants.CONFIG_APPNAME_PROP];
                }
                return (string)base[Constants.CONFIG_APP_PROP];
            }
            set { base[Constants.CONFIG_APP_PROP] = value; }
        }

        [ConfigurationProperty(Constants.CONFIG_LOGNAME_PROP, DefaultValue = Constants.CONFIG_LOGNAME_DEFAULT, IsKey = false, IsRequired = false)]
        public string LogName {
            get { return (string)base[Constants.CONFIG_LOGNAME_PROP]; }
            set { base[Constants.CONFIG_LOGNAME_PROP] = value; }
        }

        [ConfigurationProperty(Constants.CONFIG_TRACE_PROP, DefaultValue = Constants.CONFIG_TRACE_DEFAULT, IsKey = false, IsRequired = false)]
        public bool Trace {
            get { return (bool)base[Constants.CONFIG_TRACE_PROP]; }
            set { base[Constants.CONFIG_TRACE_PROP] = value; }
        }

        [ConfigurationProperty(Constants.CONFIG_USETHREAD_PROP, DefaultValue = Constants.CONFIG_USETHREAD_DEFAULT, IsKey = false, IsRequired = false)]
        public bool UseThread {
            get { return (bool)base[Constants.CONFIG_USETHREAD_PROP]; }
            set { base[Constants.CONFIG_USETHREAD_PROP] = value; }
        }

        [ConfigurationProperty(Constants.CONFIG_LOGGERS_PROP, IsDefaultCollection = false, IsRequired = true)]
        public List<LoggerElement> Loggers
        {
            get
            {
                if (_loggers != null) return _loggers;
                if (Children.ContainsKey(Constants.CONFIG_LOGGERS_PROP))
                {
                    _loggers =
                        Children[Constants.CONFIG_LOGGERS_PROP].Children.Values.Select(LoggerElement.Create).ToList();
                }
                return _loggers ??
                       (_loggers = new List<LoggerElement>(new[]
                           {
                               new LoggerElement
                                   {
                                       Name = Constants.CONFIG_LOGNAME_DEFAULT,
                                       Assembly = typeof (MSMQLogger).Assembly.FullName,
                                       Type = typeof (MSMQLogger).FullName
                                   }
                           }));
            }
        }
        private List<LoggerElement> _loggers;

        [ConfigurationProperty("exceptionPolicy", IsDefaultCollection = false, IsRequired = false)]
        public List<ExceptionPolicyElement> ExceptionPolicies
        {
            get
            {
                if (_exceptionPoliciesOverride != null) return _exceptionPoliciesOverride;
                if (Children.ContainsKey(Constants.CONFIG_EXCEPTIONPOLICY_PROP))
                {
                    _exceptionPoliciesOverride = Children[Constants.CONFIG_LOGGERS_PROP].Children.Values.Select(ExceptionPolicyElement.Create).ToList();
                }
                return _exceptionPoliciesOverride ??
                       (_exceptionPoliciesOverride = new List<ExceptionPolicyElement>(new[]
                           {
                               new ExceptionPolicyElement
                                   {
                                       Rethrow = false,
                                       Name = LoggingBoundaries.UI.ToString(),
                                       Boundary = LoggingBoundaries.UI
                                   }
                           }));
            }
        }
        private List<ExceptionPolicyElement> _exceptionPoliciesOverride;
    }
}
