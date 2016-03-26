﻿using System.Collections.Generic;
using System.Configuration;
using Civic.Core.Configuration;

namespace Civic.Core.Logging.Configuration
{
    public class LoggerElement {

        public LoggerElement()
        {
            Attributes = new Dictionary<string, string>();
        }

        public string Name { get; set; }

        public string Assembly { get; set; }

        public string Type { get; set; }

        public Dictionary<string, string> Attributes { get; set; }

        public List<string> AppliesTo { get; set; }

        public List<string> FilterBy { get; set; }

        public int RescanTime { get; set; }

        public ILogWriter Writer { get; set; }

        public static LoggerElement Create(INamedElement configElement)
        {
            if (string.IsNullOrEmpty(configElement.Name) || !configElement.Attributes.ContainsKey(Constants.CONFIG_TYPE_PROP))
                throw new ConfigurationErrorsException("projectConfig element must contain a name and a type for the log reader/writer");

            string assembly = typeof(Logger).Assembly.FullName;
            if (configElement.Attributes.ContainsKey(Constants.CONFIG_ASSEMBLY_PROP))
                assembly = configElement.Attributes[Constants.CONFIG_ASSEMBLY_PROP];

            var config = new LoggerElement
            {
                Name = configElement.Name,
                Assembly = assembly,
                Type = configElement.Attributes[Constants.CONFIG_TYPE_PROP],
                Attributes = configElement.Attributes
            };

            if (config.Attributes == null) config.Attributes = new Dictionary<string, string>();

            config.RescanTime = config.Attributes.ContainsKey(Constants.CONFIG_CHECKFORENTRIESTIME_PROP)
                ? int.Parse(config.Attributes[Constants.CONFIG_CHECKFORENTRIESTIME_PROP])
                : LoggerSection.Current.DefaultCheckForEntriesTime;

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