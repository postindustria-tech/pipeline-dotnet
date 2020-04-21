using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace FiftyOne.Pipeline.Engines.FiftyOne.FlowElements
{
    public class SequenceElementBuilder
    {
        private ILoggerFactory _loggerFactory;
        private ILogger _logger;

        public SequenceElementBuilder(
            ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
            _logger = _loggerFactory.CreateLogger<SequenceElementBuilder>();
        }

        public SequenceElementBuilder(
            ILoggerFactory loggerFactory,
            ILogger logger)
        {
            _loggerFactory = loggerFactory;
            _logger = logger;
        }

        public SequenceElement Build()
        {
            return new SequenceElement(_loggerFactory.CreateLogger<SequenceElement>());
        }
    }
}
