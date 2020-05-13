using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace FiftyOne.Pipeline.Engines.FiftyOne.FlowElements
{
    /// <summary>
    /// Builder for <see cref="SequenceElement"/> instances.
    /// Sequence Element is an element that is required by other Pipeline
    /// elements to provide certain features.
    /// In most cases, it will automatically be added to the Pipeline 
    /// when needed.
    /// </summary>
    public class SequenceElementBuilder
    {
        private ILoggerFactory _loggerFactory;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="loggerFactory">
        /// The factory to use when creating logger instances
        /// </param>
        public SequenceElementBuilder(
            ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        /// <summary>
        /// Create the <see cref="SequenceElement"/>
        /// </summary>
        /// <returns>
        /// A new <see cref="SequenceElement"/> instance
        /// </returns>
        public SequenceElement Build()
        {
            return new SequenceElement(_loggerFactory.CreateLogger<SequenceElement>());
        }
    }
}
