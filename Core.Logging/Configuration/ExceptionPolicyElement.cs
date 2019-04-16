using System;
using System.Configuration;
using Core.Configuration;

namespace Stack.Core.Logging.Configuration
{
    public class ExceptionPolicyElement
    {
        public string Name { get; set; }

        public LoggingBoundaries Boundary { get; set; }

        public bool Rethrow { get; set; }
       
        public string Type { get; set; }

        public static ExceptionPolicyElement Create(INamedElement configElement)
        {
            if (string.IsNullOrEmpty(configElement.Name) || !configElement.Attributes.ContainsKey(Constants.CONFIG_BOUNDARY_PROP))
                throw new ConfigurationErrorsException("exception element must contain a name and a boundary attributes");

            var config = new ExceptionPolicyElement
            {
                Name = configElement.Name,
                Boundary = (LoggingBoundaries)Enum.Parse(typeof(LoggingBoundaries), configElement.Attributes[Constants.CONFIG_BOUNDARY_PROP])
            };

            if (configElement.Attributes.ContainsKey(Constants.CONFIG_TYPE_PROP))
                config.Type = configElement.Attributes[Constants.CONFIG_TYPE_PROP];

            if (configElement.Attributes.ContainsKey(Constants.CONFIG_RETHROW_PROP))
                config.Rethrow = bool.Parse(configElement.Attributes[Constants.CONFIG_RETHROW_PROP]);

            return config;
        }
    }
}