using System;

namespace FiftyOne.Pipeline.Core.Exceptions
{
    /// <summary>
    /// Thrown by <see cref="FlowElements.IFlowElement.Properties"/> to indicate
    /// that properties are not available yet
    /// but MAY(!) be re-requested later.
    /// </summary>
    public class PropertiesNotYetLoadedException: PipelineException
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public PropertiesNotYetLoadedException() : base() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">
        /// The exception message
        /// </param>
        public PropertiesNotYetLoadedException(
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
        public PropertiesNotYetLoadedException(
            string message,
            Exception innerException) :
            base(message, innerException)
        { }
    }
}
