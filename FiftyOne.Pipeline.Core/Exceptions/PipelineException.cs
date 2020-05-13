using System;
using System.Collections.Generic;
using System.Text;

namespace FiftyOne.Pipeline.Core.Exceptions
{
    /// <summary>
    /// Base class for all exceptions thrown by Pipeline API components
    /// </summary>
    public class PipelineException : Exception
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public PipelineException() : base() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">
        /// The exception message
        /// </param>
        public PipelineException(
            string message)
            : base(message)
        { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">
        /// The exception message
        /// </param>
        /// <param name="innerException">
        /// The inner exception that triggered this exception.
        /// </param>
        public PipelineException(
            string message,
            Exception innerException) :
            base(message, innerException)
        { }
    }
}
