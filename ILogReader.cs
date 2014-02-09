namespace Civic.Core.Logging
{
    /// <summary>
    /// Describes a log reader
    /// </summary>
    public interface ILogReader
    {
        bool HasMessage { get; }

        ILogMessage Receive();
    }
}