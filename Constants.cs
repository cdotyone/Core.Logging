namespace Civic.Core.Logging
{
    internal class Constants
    {
        public const string CONFIG_EXCEPTIONPOLICY_PROP = "exceptionPolicy";

        public const string CONFIG_APP_PROP = "app";
        public const string CONFIG_CLIENT_PROP = "client";
        public const string CONFIG_ENV_PROP = "env";

        public const string CONFIG_APPNAME_PROP = "appname";
        public const string CONFIG_RETHROW_PROP = "rethrow";
        public const string CONFIG_BOUNDARY_PROP = "boundary";

        public const string CONFIG_LOGNAME_PROP = "logname";
        public const string CONFIG_LOGNAME_DEFAULT = "civic";

        public const string CONFIG_USETHREAD_PROP = "useThread";
        public const bool CONFIG_USETHREAD_DEFAULT = false;

        public const string CONFIG_TRACE_PROP = "trace";
        public const bool CONFIG_TRACE_DEFAULT = false;

        public const string CONFIG_LOGGERS_PROP = "loggers";

        public const string CORE_LOGGING_SECTION = "coreLogging";
        public const string CONFIG_ASSEMBLY_PROP = "assembly";
        public const string CONFIG_TYPE_PROP = "type";


        // added to combine configuration sections to one place for all logging services
        public const string CONFIG_FILTERBY_PROP = "filterBy";
        public const string CONFIG_APPLIESTO_PROP = "appliesTo";
        public const string CONFIG_WRITERS_PROP = "writers";
        public const string CONFIG_READERS_PROP = "readers";

        // the # minutes before the service rescans the domain to get a fresh cache
        public const string CONFIG_RECHECKMINUTES_PROP = "recheckMinutes";
        public const int CONFIG_RECHECKMINUTES_DEFAULT = 5;

        public const string CONFIG_CHECKFORENTRIESTIME_PROP = "rescanTime";
        public const int CONFIG_CHECKFORENTRIESTIME_DEFAULT = 30000;

    }
}
