namespace Civic.Core.Logging
{
    internal class Constants
    {
        public const string CONFIG_EXCEPTIONPOLICY_PROP = "exceptionPolicy";

        public const string CONFIG_RETHROW_PROP = "rethrow";
        public const string CONFIG_BOUNDARY_PROP = "boundary";

        public const string CONFIG_LOGNAME_PROP = "logname";
        public const string CONFIG_LOGNAME_DEFAULT = "civic";
        public const string CONFIG_USETHREAD_PROP = "useThread";
        public const string CONFIG_TRACE_PROP = "trace";
        public const string CONFIG_TRANSMISSION_PROP = "transmission";
        
        public const string CONFIG_APPNAME_PROP = "appname";

        public const string CONFIG_LOGGERS_PROP = "loggers";

        public const string CORE_LOGGING_SECTION = "logging";
        public const string CONFIG_ASSEMBLY_PROP = "assembly";
        public const string CONFIG_FAILURERECOVERY_PROP = "failureRecovery";
        public const string CONFIG_TYPE_PROP = "type";


        // added to combine configuration sections to one place for all logging services
        public const string CONFIG_EXCLUDESEVERITY_PROP = "excludeSeverity";
        public const string CONFIG_EXCLUDEBOUNDARY_PROP = "excludeBondary";
        public const string CONFIG_WRITERS_PROP = "writers";
        public const string CONFIG_READERS_PROP = "readers";

        // the # minutes before the service rescans the domain to get a fresh cache
        public const string CONFIG_CONFIGCHECKMINUTES_PROP = "recheckMinutes";
        public const int CONFIG_CONFIGCHECKMINUTES_DEFAULT = 5;

        public const string CONFIG_RESCANTIME_PROP = "rescanTime";
        public const int CONFIG_RESCANTIME_DEFAULT = 30000;

        public const string CONFIG_RECOVERYTIME_PROP = "recoveryTime";
        public const int CONFIG_RECOVERYTIME_DEFAULT = 120000;

    }
}
