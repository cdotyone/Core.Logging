using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.InteropServices;
using Civic.Core.Configuration;
using Civic.Core.Logging.LogWriters;

namespace Civic.Core.Logging.Configuration {

    public class LoggingConfig : NamedConfigurationElement
    {
        private static CivicSection _coreConfig;
        private static LoggingConfig _current;
        private List<LoggerConfig> _loggers;
        private List<ExceptionPolicyElement> _exceptionPoliciesOverride;
        private int _configChangeCheck;
        private int _defaultCheckForEntriesTime;
        private string _clientCode;
        private string _environmentCode;
        private string _applicationName;
        private string _logName;
        private bool _trace;
        private bool _useThread;

        public LoggingConfig(INamedElement element)
        {
            if (element == null) element = new NamedConfigurationElement() { Name = SectionName };
            Children = element.Children;
            Attributes = element.Attributes;
            Name = element.Name;

            var civicSection = CivicSection.Current;

            _configChangeCheck = Attributes.ContainsKey(Constants.CONFIG_RECHECKMINUTES_PROP) ? int.Parse(Attributes[Constants.CONFIG_RECHECKMINUTES_PROP]) : Constants.CONFIG_RECHECKMINUTES_DEFAULT;
            _defaultCheckForEntriesTime = Attributes.ContainsKey(Constants.CONFIG_CHECKFORENTRIESTIME_PROP) ? int.Parse(Attributes[Constants.CONFIG_CHECKFORENTRIESTIME_PROP]) : Constants.CONFIG_CHECKFORENTRIESTIME_DEFAULT;
            _applicationName = GetAttribute(Constants.CONFIG_APPNAME_PROP, civicSection.ApplicationName);
            _clientCode = civicSection.ClientCode;
            _environmentCode = civicSection.EnvironmentCode;
            _logName = Attributes.ContainsKey(Constants.CONFIG_LOGNAME_PROP) ? Attributes[Constants.CONFIG_LOGNAME_PROP] : Constants.CONFIG_LOGNAME_DEFAULT;
            _trace = Attributes.ContainsKey(Constants.CONFIG_TRACE_PROP) && bool.Parse(Attributes[Constants.CONFIG_TRACE_PROP]);
            _useThread = Attributes.ContainsKey(Constants.CONFIG_USETHREAD_PROP) && bool.Parse(Attributes[Constants.CONFIG_USETHREAD_PROP]);
        }

        /// <summary>
        /// The current configuration for caching module
        /// </summary>
        public static LoggingConfig Current
        {
            get
            {
                if (_current != null) return _current;
                if (_coreConfig == null) _coreConfig = CivicSection.Current;
                _current = new LoggingConfig(_coreConfig.Children.ContainsKey(SectionName) ? _coreConfig.Children[SectionName] : null);
                return _current;
            }
        }

        public static string SectionName
        {
            get { return Constants.CORE_LOGGING_SECTION; }
        }

        /// <summary>
        /// Gets or sets the # minutes before the checks for config changes
        /// </summary>
        public int ConfigChangeCheck
        {
            get { return _configChangeCheck; }
            set { _configChangeCheck = value; Attributes[Constants.CONFIG_RECHECKMINUTES_PROP] = value.ToString(); }
        }

        /// <summary>
        /// Gets or sets the default time before the threads rescans for entries
        /// </summary>
        public int DefaultCheckForEntriesTime
        {
            get { return _defaultCheckForEntriesTime; }
            set { _defaultCheckForEntriesTime = value; Attributes[Constants.CONFIG_CHECKFORENTRIESTIME_PROP] = value.ToString(); }
        }

        public string ClientCode
        {
            get { return _clientCode; }
        }

        public string EnvironmentCode
        {
            get
            {
                if (string.IsNullOrEmpty(_environmentCode)) return "PROD";
                return _environmentCode; 
            }
        }

        public string ApplicationName
        {
            get { return _applicationName; }
            set { _applicationName = value; Attributes[Constants.CONFIG_APPNAME_PROP] = value; }
        }

        public string LogName
        {
            get { return _logName; }
            set { _logName = value; Attributes[Constants.CONFIG_LOGNAME_PROP] = value; }
        }


        public bool Trace
        {
            get { return _trace; }
            set { _trace = value; base[Constants.CONFIG_TRACE_PROP] = value; }
        }

        public bool UseThread
        {
            get { return _useThread; }
            set { _useThread = value; base[Constants.CONFIG_USETHREAD_PROP] = value; }
        }

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

        /// <summary>
        /// Gets the collection log readers that must be called
        /// </summary>
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


        private string GetAttribute(string name, string defaultValue)
        {
            if (Attributes.ContainsKey(name)) return Attributes[name];
            if (string.IsNullOrEmpty(ConfigurationManager.AppSettings[name])) return defaultValue;
            return ConfigurationManager.AppSettings[name];
        }
    }
}
