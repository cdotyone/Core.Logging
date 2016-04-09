using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Civic.Core.Configuration;
using Civic.Core.Logging.LogWriters;

namespace Civic.Core.Logging.Configuration {

    public class LoggingConfig : NamedConfigurationElement
    {
        public LoggingConfig(INamedElement element)
        {
            if (element == null) element = new NamedConfigurationElement() { Name = SectionName };
            Children = element.Children;
            Attributes = element.Attributes;
            Name = element.Name;
        }

        /// <summary>
        /// The current configuration for caching module
        /// </summary>
        public static LoggingConfig Current
        {
            get
            {
                if (_coreConfig == null) _coreConfig = CivicSection.Current;
                _current = new LoggingConfig(_coreConfig.Children.ContainsKey(SectionName) ? _coreConfig.Children[SectionName] : null);
                return _current;
            }
        }
        private static CivicSection _coreConfig;
        private static LoggingConfig _current;


        public static string SectionName
        {
            get { return Constants.CORE_LOGGING_SECTION; }
        }

        /// <summary>
        /// Gets or sets the # minutes before the checks for config changes
        /// </summary>
        public int ConfigChangeCheck
        {
            get { return Attributes.ContainsKey(Constants.CONFIG_RECHECKMINUTES_PROP) ? int.Parse(Attributes[Constants.CONFIG_RECHECKMINUTES_PROP]) : Constants.CONFIG_RECHECKMINUTES_DEFAULT; }
            set { Attributes[Constants.CONFIG_RECHECKMINUTES_PROP] = value.ToString(); }
        }

        /// <summary>
        /// Gets or sets the default time before the threads rescans for entries
        /// </summary>
        public int DefaultCheckForEntriesTime
        {
            get { return Attributes.ContainsKey(Constants.CONFIG_CHECKFORENTRIESTIME_PROP) ? int.Parse(Attributes[Constants.CONFIG_CHECKFORENTRIESTIME_PROP]) : Constants.CONFIG_CHECKFORENTRIESTIME_DEFAULT; }
            set { Attributes[Constants.CONFIG_CHECKFORENTRIESTIME_PROP] = value.ToString(); }
        }

        public string ClientCode
        {
            get
            {
                if (Attributes.ContainsKey(Constants.CONFIG_CLIENTCODE_PROP)) return Attributes[Constants.CONFIG_CLIENTCODE_PROP];
                if (string.IsNullOrEmpty(ConfigurationManager.AppSettings[Constants.CONFIG_CLIENTCODE_PROP])) return "CIVIC";
                return ConfigurationManager.AppSettings[Constants.CONFIG_CLIENTCODE_PROP];
            }
            set { Attributes[Constants.CONFIG_CLIENTCODE_PROP] = value; }
        }

        public string EnvironmentCode
        {
            get
            {
                if (Attributes.ContainsKey(Constants.CONFIG_ENVCODE_PROP)) return Attributes[Constants.CONFIG_ENVCODE_PROP];
                if (string.IsNullOrEmpty(ConfigurationManager.AppSettings[Constants.CONFIG_ENVCODE_PROP])) return "PROD";
                return ConfigurationManager.AppSettings[Constants.CONFIG_ENVCODE_PROP];
            }
            set { Attributes[Constants.CONFIG_APP_PROP] = value; }
        }

        public string ApplicationName
        {
            get
            {
                if (Attributes.ContainsKey(Constants.CONFIG_APP_PROP)) return Attributes[Constants.CONFIG_APP_PROP];
                if (string.IsNullOrEmpty(ConfigurationManager.AppSettings[Constants.CONFIG_APP_PROP])) return "Unknown";
                return ConfigurationManager.AppSettings[Constants.CONFIG_APP_PROP];
            }
            set { Attributes[Constants.CONFIG_APP_PROP] = value; }
        }

        public string LogName {
            get { return Attributes.ContainsKey(Constants.CONFIG_LOGNAME_PROP) ? Attributes[Constants.CONFIG_LOGNAME_PROP] : Constants.CONFIG_LOGNAME_DEFAULT; }
            set { Attributes[Constants.CONFIG_LOGNAME_PROP] = value; }
        }

        public bool Trace {
            get { return Attributes.ContainsKey(Constants.CONFIG_TRACE_PROP) && bool.Parse(Attributes[Constants.CONFIG_TRACE_PROP]); }
            set { base[Constants.CONFIG_TRACE_PROP] = value; }
        }

        public bool UseThread {
            get { return Attributes.ContainsKey(Constants.CONFIG_USETHREAD_PROP) && bool.Parse(Attributes[Constants.CONFIG_USETHREAD_PROP]); }
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
                    return _loggers = Children[Constants.CONFIG_LOGGERS_PROP].Children.Values.Select(LoggerConfig.Create).ToList();
                }
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
