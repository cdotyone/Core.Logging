#region Copyright / Comments

// <copyright file="LogSeverity.cs" company="Civic Engineering & IT">Copyright � Civic Engineering & IT 2013</copyright>
// <author>Chris Doty</author>
// <email>dotyc@civicinc.com</email>
// <date>6/4/2013</date>
// <summary></summary>

#endregion Copyright / Comments

namespace Core.Logging
{
    /// <summary>
    /// Defines different message severities
    /// </summary>
    public enum LogSeverity
    {
        Exception,
        Error,
        Warning,
        Information,
        Trace,
        Transmission
    }
}