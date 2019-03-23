#region Copyright / Comments

// <copyright file="PerformanceTracers.cs" company="Civic Engineering & IT">Copyright © Civic Engineering & IT 2013</copyright>
// <author>Chris Doty</author>
// <email>dotyc@civicinc.com</email>
// <date>6/4/2013</date>
// <summary></summary>

#endregion Copyright / Comments

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Security;
using System.Security.Permissions;

namespace Core.Logging
{
    internal class PerformanceTracer : IDisposable
    {
        #region Members

        private Stopwatch _stopwatch;
        private long _tracingStartTicks;
        private bool _tracerDisposed;
        private bool _tracingAvailable;
		private readonly LoggingBoundaries _boundary;
    	private readonly string _operation;

        #endregion

        public PerformanceTracer(LoggingBoundaries boundary, params object[] parameterValues)
        {
            _boundary = boundary;

            string operation = "Unknown";
            
            if (parameterValues.Length > 0 && parameterValues[0] is string)
            {
                operation = parameterValues[0].ToString();
            }
            if (parameterValues.Length > 0 && parameterValues[0] is Type)
            {
                operation = ((Type)parameterValues[0]).Name;
            }
            for (var i = 1; i < parameterValues.Length; i++)
            {
                if (parameterValues[i] is Type)
                    operation += "." + ((Type) parameterValues[i]).Name;
                else
                    operation += "." + Convert.ToString(parameterValues[i]);
            }

            _operation = operation;

            if (checkTracingAvailable())
            {
                if (getActivityID().Equals(Guid.Empty))
                {
                    setActivityID(Guid.NewGuid());
                }

                initialize(operation);
            }
        }

        /// <summary>
        /// <para>Releases unmanaged resources and performs other cleanup operations before the <see cref="PerformanceTracer"/> is 
		/// reclaimed by garbage collection</para>
		/// </summary>
		~PerformanceTracer()
		{
			Dispose(false);
		}

		/// <summary>
        /// Causes the <see cref="PerformanceTracer"/> to output its closing message.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
        /// <para>Releases the unmanaged resources used by the <see cref="PerformanceTracer"/> and optionally releases 
		/// the managed resources.</para>
		/// </summary>
		/// <param name="disposing">
		/// <para><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> 
		/// to release only unmanaged resources.</para>
		/// </param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing && !_tracerDisposed)
			{
				if (_tracingAvailable)
				{
					try
					{
                        if (Logger.IsTraceOn) writeTraceEndMessage();
					}
					finally
					{
						try
						{
							stopLogicalOperation();
						}
						catch (SecurityException)
						{
						}
					}
				}

				_tracerDisposed = true;
			}
		}

		public static bool IsTracingAvailable()
		{
			bool tracingAvailable = false;

			try
			{
                var permissionSet = new PermissionSet(PermissionState.None);
                permissionSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.UnmanagedCode));

                #if NETFULL
                tracingAvailable = permissionSet.IsSubsetOf(AppDomain.CurrentDomain.PermissionSet);
                #else
                tracingAvailable = false; // not sure so we will turn it off for now
                #endif
            }
			catch (SecurityException)
			{ }

			return tracingAvailable;
		}

		private bool checkTracingAvailable()
		{
			_tracingAvailable = IsTracingAvailable();
			return _tracingAvailable;
		}

        private void initialize(string operation)
        {
            startLogicalOperation(operation);
            if (Logger.IsTraceOn)
            {
                _stopwatch = Stopwatch.StartNew();
                _tracingStartTicks = Stopwatch.GetTimestamp();
            }
        }

		private void writeTraceEndMessage()
		{
			long tracingEndTicks = Stopwatch.GetTimestamp();
			decimal secondsElapsed = GetSecondsElapsed(_stopwatch.ElapsedMilliseconds);

			string methodName = getExecutingMethodName();
            string message = string.Format("[elapsed time: {4} seconds]\r\nTrace Information: Activity '{0}' in method '{1}' started at: {2} ticks and ended at: {3} ticks", getActivityID(), methodName, _tracingStartTicks, tracingEndTicks, secondsElapsed);
			writeTraceMessage(message);
		}

		private void writeTraceMessage(string message)
		{
			var extendedProperties = new Dictionary<string, object>
			                         	{
			                         		{"Duration", GetSecondsElapsed(_stopwatch.ElapsedMilliseconds)},
			                         		{"OperationName", _operation},
			                         		{"OperationStack", peekLogicalOperationStack()}
			                         	};
            var entry = new LogMessage(_boundary, LogSeverity.Trace, message) { Extended = extendedProperties };
		    Logger.Log(entry);
		}

		private string getExecutingMethodName()
		{
			string result = "Unknown";
			var trace = new StackTrace(false);

			for (int index = 0; index < trace.FrameCount; ++index)
			{
				StackFrame frame = trace.GetFrame(index);
				MethodBase method = frame.GetMethod();
				if (method.DeclaringType != GetType())
				{
					if (method.DeclaringType != null) result = string.Concat(method.DeclaringType.FullName, ".", method.Name);
					break;
				}
			}

			return result;
		}

		private decimal GetSecondsElapsed(long milliseconds)
		{
			decimal result = Convert.ToDecimal(milliseconds) / 1000m;
			return Math.Round(result, 6);
		}

		private static Guid getActivityID()
		{
			return Trace.CorrelationManager.ActivityId;
		}

		private static void setActivityID(Guid activityId)
		{
			Trace.CorrelationManager.ActivityId = activityId;
		}

    	private static void startLogicalOperation(string operation)
		{
			Trace.CorrelationManager.StartLogicalOperation(operation);
		}

		private static void stopLogicalOperation()
		{
			Trace.CorrelationManager.StopLogicalOperation();
		}

		private static object peekLogicalOperationStack()
		{
			return Trace.CorrelationManager.LogicalOperationStack.Peek();
		}
	}
}
