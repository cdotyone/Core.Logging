using System.Collections.Generic;
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

        public Dictionary<string, string> Attributes { get; private set; }

        public static LoggerElement Create(INamedElement configElement)
        {
            if (string.IsNullOrEmpty(configElement.Name) || !configElement.Attributes.ContainsKey(Constants.CONFIG_TYPE_PROP))
                throw new ConfigurationErrorsException("loggerElement element must contain a name and a type for the log writer");

            string assembly = typeof (Logger).Assembly.FullName;
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

            return config;
        }
    
    }
}