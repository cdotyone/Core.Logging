using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Logging;
using Core.Logging.Configuration;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Newtonsoft.Json;

namespace Logging.AppInsight
{
    public class AppInsightWriter : ILogWriter
    {
        private static readonly TelemetryConfiguration _config = TelemetryConfiguration.CreateDefault();
        TelemetryClient _telemetry = new TelemetryClient(_config);

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

        public object Create(string applicationName, string logName, LoggerConfig config)
        {
            return new AppInsightWriter
            {
                LogName = logName,
                ApplicationName = applicationName
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
