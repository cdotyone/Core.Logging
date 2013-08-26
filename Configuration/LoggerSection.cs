using Civic.Core.Configuration;

namespace Civic.Core.Logging.Configuration {

    public class LoggerSection : System.Configuration.ConfigurationSection {
        
        [System.Configuration.ConfigurationProperty("app", IsKey=false, IsRequired=true)]
        public string App {
            get {
                return ((string)(base["app"]));
            }
            set {
                base["app"] = value;
            }
        }
        
        [System.Configuration.ConfigurationProperty("logname", DefaultValue="PO", IsKey=false, IsRequired=false)]
        public string LogName {
            get {
                return ((string)(base["logname"]));
            }
            set {
                base["logname"] = value;
            }
        }
        
        [System.Configuration.ConfigurationProperty("trace", DefaultValue=false, IsKey=false, IsRequired=false)]
        public bool Trace {
            get {
                return ((bool)(base["trace"]));
            }
            set {
                base["trace"] = value;
            }
        }

        [System.Configuration.ConfigurationProperty("useThread", DefaultValue = false, IsKey = false, IsRequired = false)]
        public bool UseThread
        {
            get
            {
                return ((bool)(base["useThread"]));
            }
            set
            {
                base["useThread"] = value;
            }
        }

        [System.Configuration.ConfigurationProperty("loggers", IsDefaultCollection = false, IsRequired = true)]
        public NamedElementCollection<LoggerElement> Loggers {
            get {
                return ((NamedElementCollection<LoggerElement>)(base["loggers"]));
            }
        }

        [System.Configuration.ConfigurationProperty("exceptionPolicy", IsDefaultCollection = false, IsRequired = false)]
        public NamedElementCollection<ExceptionPolicyElement> ExceptionPolicies
        {
            get
            {
                var policies = ((NamedElementCollection<ExceptionPolicyElement>)(base["exceptionPolicy"]));
                if (policies != null && policies.Count > 0) return policies;

                if(_exceptionPoliciesOverride==null)
                    _exceptionPoliciesOverride = new NamedElementCollection<ExceptionPolicyElement>
                               {
                                   new ExceptionPolicyElement
                                       {
                                           Rethrow = false,
                                           Name = LoggingBoundaries.UI.ToString(),
                                           Boundary = LoggingBoundaries.UI
                                       }
                               };
                return _exceptionPoliciesOverride;
            }
        }
        private NamedElementCollection<ExceptionPolicyElement> _exceptionPoliciesOverride;
    }
}
