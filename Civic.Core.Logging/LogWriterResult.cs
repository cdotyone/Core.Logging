using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Civic.Core.Logging
{
    public class LogWriterResult
    {
        /// <summary>
        /// True if log entry was sent succesfully
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Name of log provider that returned the result
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The message that failed
        /// </summary>
        public ILogMessage Message { get; set; }
    }
}
