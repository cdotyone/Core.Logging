using Civic.Core.Configuration;

namespace Civic.Core.Logging.Configuration
{
    public class LoggerElement : NamedConfigurationElement {
        [System.Configuration.ConfigurationProperty("type", DefaultValue="", IsKey=false, IsRequired=false)]
        public string Type {
            get {
                return ((string)(base["type"]));
            }
            set {
                base["type"] = value;
            }
        }

        [System.Configuration.ConfigurationProperty("param")]
        public NamedElementCollection<NamedConfigurationElement> Params
        {
            get
            {
                return ((NamedElementCollection<NamedConfigurationElement>)(base["param"]));
            }
        }
    }
}