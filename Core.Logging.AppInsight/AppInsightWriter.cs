using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Logging.Configuration;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Newtonsoft.Json;

namespace Core.Logging.AppInsight
{
    public class AppInsightWriter : ILogWriter
    {
        TelemetryClient _telemetry = new TelemetryClient();

        #region Properties

        /// <summary>
        /// gets the name given to this log
        /// </summary>
        public string ApplicationName { get; private set; }

        /// <summary>
        /// true if the ILogWriter supports a delete command
        /// false if it does not
        /// </summary>
        public bool CanDelete
        {
            get { return true; }
        }

        /// <summary>
        /// gets the name given to this log
        /// </summary>
        public string LogName { get; private set; }

        /// <summary>
        /// gets the name given to this log
        /// </summary>
        public string Name
        {
            get { return "Application Insight Logger"; }
        }

        #endregion Properties

        public object Create(string applicationname, string logname, LoggerConfig config)
        {
            return new AppInsightWriter
            {
                LogName = logname,
                ApplicationName = applicationname
            };
        }

        public void Delete()
        {
            _telemetry = null;
        }

        public void Flush()
        {
            _telemetry.Flush();
        }

        public Task<LogWriterResult> Log(ILogMessage message)
        {
            return new Task<LogWriterResult>(delegate
            {
                if (string.IsNullOrEmpty(message.ApplicationName)) message.ApplicationName = ApplicationName;

                try
                {
                    var extended = new Dictionary<string, string>();

                    foreach (var pair in message.Extended)
                    {
                        if (pair.Value == null) continue;

                        if (pair.Value is string || pair.Value is long || pair.Value is int || pair.Value is float || pair.Value is decimal || pair.Value is DateTime || pair.Value is Guid)
                        {
                            extended[pair.Key] = pair.Value.ToString();
                            continue;
                        }

                        extended[pair.Key] = JsonConvert.SerializeObject(pair.Value);
                    }

                    switch (message.Type)
                    {
                        case LogSeverity.Exception:
                            _telemetry.TrackTrace(message.Message,
                                SeverityLevel.Critical,
                                extended);
                            break;
                        case LogSeverity.Information:
                            _telemetry.TrackTrace(message.Message,
                                SeverityLevel.Information,
                                extended);
                            break;
                        case LogSeverity.Trace:
                            _telemetry.TrackTrace(message.Message,
                                SeverityLevel.Verbose,
                                extended);
                            break;
                        case LogSeverity.Warning:
                            _telemetry.TrackTrace(message.Message,
                                SeverityLevel.Warning,
                                extended);
                            break;
                        case LogSeverity.Transmission:
                            _telemetry.TrackTrace(message.Message,
                                SeverityLevel.Verbose,
                                extended);
                            break;
                        default:
                            _telemetry.TrackTrace(message.Message,
                                SeverityLevel.Error,
                                extended);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Logger.HandleException(LoggingBoundaries.Unknown, ex);
                    return new LogWriterResult { Success = false, Name = Name, Message = message };
                }

                return new LogWriterResult { Success = true, Name = Name }; 
            });

           
        }
    }
}
