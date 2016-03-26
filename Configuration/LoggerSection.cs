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

        /// <summary>
        /// Gets or sets the # minutes before the checks for config changes
        /// </summary>
        [ConfigurationProperty(Constants.CONFIG_RECHECKMINUTES_PROP, IsRequired = false, DefaultValue = Constants.CONFIG_RECHECKMINUTES_DEFAULT)]
        public int ConfigChangeCheck
        {
            get { return (int)base[Constants.CONFIG_RECHECKMINUTES_PROP]; }
            set { base[Constants.CONFIG_RECHECKMINUTES_PROP] = value; }
        }

        /// <summary>
        /// Gets or sets the default time before the threads rescans for entries
        /// </summary>
        [ConfigurationProperty(Constants.CONFIG_CHECKFORENTRIESTIME_PROP, IsRequired = false, DefaultValue = Constants.CONFIG_CHECKFORENTRIESTIME_DEFAULT)]
        public int DefaultCheckForEntriesTime
        {
            get { return (int)base[Constants.CONFIG_CHECKFORENTRIESTIME_PROP]; }
            set { base[Constants.CONFIG_CHECKFORENTRIESTIME_PROP] = value; }
        }

        [ConfigurationProperty(Constants.CONFIG_APP_PROP, IsKey = false, IsRequired = true)]
        public string ClientCode
        {
            get
            {
                if (string.IsNullOrEmpty((string)base[Constants.CONFIG_APP_PROP]))
                {
                    if (string.IsNullOrEmpty(ConfigurationManager.AppSettings[Constants.CONFIG_APPNAME_PROP])) return "CIVIC";
                    return ConfigurationManager.AppSettings[Constants.CONFIG_APPNAME_PROP];
                }
                return (string)base[Constants.CONFIG_APP_PROP];
            }
            set { base[Constants.CONFIG_APP_PROP] = value; }
        }

        [ConfigurationProperty(Constants.CONFIG_APP_PROP, IsKey = false, IsRequired = true)]
        public string EnvironmentCode
        {
            get
            {
                if (string.IsNullOrEmpty((string)base[Constants.CONFIG_APP_PROP]))
                {
                    if (string.IsNullOrEmpty(ConfigurationManager.AppSettings[Constants.CONFIG_APPNAME_PROP])) return "PROD";
                    return ConfigurationManager.AppSettings[Constants.CONFIG_APPNAME_PROP];
                }
                return (string)base[Constants.CONFIG_APP_PROP];
            }
            set { base[Constants.CONFIG_APP_PROP] = value; }
        }

        [ConfigurationProperty(Constants.CONFIG_APP_PROP, IsKey = false, IsRequired = true)]
        public string ApplicationName
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
        public List<LoggerConfig> Loggers
        {
            get
            {
                if (_loggers != null) return _loggers;
                if (Children.ContainsKey(Constants.CONFIG_LOGGERS_PROP))
                {
                    _loggers = Children[Constants.CONFIG_LOGGERS_PROP].Children.Values.Select(LoggerConfig.Create).ToList();
                }
                if (_loggers == null)
                {
                    if (System.Diagnostics.Debugger.IsAttached)
                    {
                        _loggers = new List<LoggerConfig>(new[]
                            {
                                new LoggerConfig
                                    {
                                        Name = Constants.CONFIG_LOGNAME_DEFAULT,
                                        Assembly = typeof (MSMQLogger).Assembly.FullName,
                                        Type = typeof (MSMQLogger).FullName
                                    },
                                new LoggerConfig
                                    {
                                        Name = "DEBUG",
                                        Assembly = typeof (DebugLogger).Assembly.FullName,
                                        Type = typeof (DebugLogger).FullName
                                    }

                            });
                    }
                    else
                    {
                        _loggers = new List<LoggerConfig>(new[]
                            {
                                new LoggerConfig
                                    {
                                        Name = Constants.CONFIG_LOGNAME_DEFAULT,
                                        Assembly = typeof (MSMQLogger).Assembly.FullName,
                                        Type = typeof (MSMQLogger).FullName
                                    }
                            });
                    }
                }
                return _loggers;
            }
        }
        private List<LoggerConfig> _loggers;


        /// <summary>
        /// Gets the collection log readers that must be called
        /// </summary>
        [ConfigurationProperty(Constants.CONFIG_READERS_PROP, IsDefaultCollection = false)]
        public List<LoggerConfig> Readers
        {
            get
            {
                return Children.ContainsKey(Constants.CONFIG_READERS_PROP) ? Children[Constants.CONFIG_READERS_PROP].Children.Values.Select(LoggerConfig.Create).ToList() : new List<LoggerConfig>();
            }
        }

        /// <summary>
        /// Gets the collection log writers that must be called
        /// </summary>
        [ConfigurationProperty(Constants.CONFIG_WRITERS_PROP, IsDefaultCollection = false)]
        public List<LoggerConfig> Writers
        {
            get
            {
                return Children.ContainsKey(Constants.CONFIG_WRITERS_PROP) ? Children[Constants.CONFIG_WRITERS_PROP].Children.Values.Select(LoggerConfig.Create).ToList() : new List<LoggerConfig>();
            }
        }

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
