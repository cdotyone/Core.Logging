using System.Collections.Generic;
using System.Configuration;
using Civic.Core.Configuration;

namespace Civic.Core.Logging.Configuration
{

    public class LoggerConfig {

        public LoggerConfig()
        {
            Attributes = new Dictionary<string, string>();
            UseFailureRecovery = true;
        }

        public string Name { get; set; }

        public string Assembly { get; set; }

        public string Type { get; set; }

        public bool UseThread { get; set; }

        public bool UseFailureRecovery { get; set; }

        public Dictionary<string, string> Attributes { get; set; }

        public List<string> AppliesTo { get; set; }

        public List<string> FilterBy { get; set; }

        public int RescanTime { get; set; }

        public int RecoveryTime { get; set; }

        public ILogWriter Writer { get; set; }

        public static LoggerConfig Create(LoggerConfig from)
        {
            return new LoggerConfig
            {
                Name = from.Name,
                Assembly = from.Assembly,
                Type = from.Type,
                UseThread = from.UseThread,
                UseFailureRecovery = from.UseFailureRecovery,
                Attributes = from.Attributes.Clone(),
                AppliesTo = new List<string>(from.AppliesTo.Clone()),
                FilterBy = new List<string>(from.FilterBy.Clone()),
                RescanTime = from.RescanTime,
                RecoveryTime = from.RecoveryTime
            };
        }
        
        public static LoggerConfig Create(INamedElement configElement)
        {
            if (string.IsNullOrEmpty(configElement.Name) || !configElement.Attributes.ContainsKey(Constants.CONFIG_TYPE_PROP))
                throw new ConfigurationErrorsException("projectConfig element must contain a name and a type for the log reader/writer");

            string assembly = typeof(Logger).Assembly.FullName;
            if (configElement.Attributes.ContainsKey(Constants.CONFIG_ASSEMBLY_PROP))
                assembly = configElement.Attributes[Constants.CONFIG_ASSEMBLY_PROP];

            var useFailureRecovery = false;
            if (configElement.Attributes.ContainsKey(Constants.CONFIG_FAILURERECOVERY_PROP))
                useFailureRecovery = bool.Parse(configElement.Attributes[Constants.CONFIG_FAILURERECOVERY_PROP]);

            var config = new LoggerConfig
            {
                Name = configElement.Name,
                Assembly = assembly,
                Type = configElement.Attributes[Constants.CONFIG_TYPE_PROP],
                Attributes = configElement.Attributes,
                UseFailureRecovery = useFailureRecovery
            };

            if (config.Attributes == null) config.Attributes = new Dictionary<string, string>();

            config.UseThread = config.Attributes.ContainsKey(Constants.CONFIG_USETHREAD_PROP)
                ? bool.Parse(config.Attributes[Constants.CONFIG_USETHREAD_PROP])
                : LoggingConfig.Current.UseThread;

            config.RescanTime = config.Attributes.ContainsKey(Constants.CONFIG_RESCANTIME_PROP)
                ? int.Parse(config.Attributes[Constants.CONFIG_RESCANTIME_PROP])
                : LoggingConfig.Current.DefaultRescanTime;

            config.RecoveryTime = config.Attributes.ContainsKey(Constants.CONFIG_RECOVERYTIME_PROP)
                ? int.Parse(config.Attributes[Constants.CONFIG_RECOVERYTIME_PROP])
                : LoggingConfig.Current.DefaultRecoveryTime;

            config.FilterBy = configElement.Attributes.ContainsKey(Constants.CONFIG_FILTERBY_PROP) ?
                        new List<string>(configElement.Attributes[Constants.CONFIG_FILTERBY_PROP].Split(','))
                      : new List<string>();

            config.AppliesTo = configElement.Attributes.ContainsKey(Constants.CONFIG_APPLIESTO_PROP) ?
                        new List<string>(configElement.Attributes[Constants.CONFIG_APPLIESTO_PROP].Split(','))
                      : new List<string>();

            return config;
        }

    }
}