using Civic.Core.Configuration;

namespace Civic.Core.Logging.Configuration
{
    public class ExceptionPolicyElement : NamedConfigurationElement
    {       
        [System.Configuration.ConfigurationProperty("boundary", IsKey = false, IsRequired = true)]
        public LoggingBoundaries Boundary
        {
            get
            {
                return ((LoggingBoundaries)(base["boundary"]));
            }
            set
            {
                base["boundary"] = value;
            }
        }

        [System.Configuration.ConfigurationProperty("rethrow", DefaultValue = true, IsKey = false)]
        public bool Rethrow
        {
            get
            {
                return ((bool)(base["rethrow"]));
            }
            set
            {
                base["rethrow"] = value;
            }
        }        

        [System.Configuration.ConfigurationProperty("type", DefaultValue="", IsKey=false, IsRequired=false)]
        public string Type {
            get {
                return ((string)(base["type"]));
            }
            set {
                base["type"] = value;
            }
        }
    }
}