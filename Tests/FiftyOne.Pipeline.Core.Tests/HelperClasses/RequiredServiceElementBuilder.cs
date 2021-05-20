using FiftyOne.Pipeline.Core.Attributes;
using FiftyOne.Pipeline.Core.Services;
using FiftyOne.Pipeline.Engines.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace FiftyOne.Pipeline.Core.Tests.HelperClasses
{
    [AlternateName("RequiredService")]
    public class RequiredServiceElementBuilder
    {
        public class EmptyService
        {

        }

        public ILoggerFactory LoggerFactory { get; private set; } = null;
        public EmptyService Service { get; private set; } = null;
        public IDataUpdateService UpdateService { get; private set; } = null;

        public RequiredServiceElementBuilder(
            ILoggerFactory loggerFactory,
            EmptyService service)
        {
            LoggerFactory = loggerFactory;
            Service = service;
        }
        public RequiredServiceElementBuilder(
            ILoggerFactory loggerFactory,
            EmptyService service,
            IDataUpdateService updateService)
        {
            LoggerFactory = loggerFactory;
            Service = service;
            UpdateService = updateService;
        }
        public RequiredServiceElementBuilder(ILoggerFactory loggerFactory)
        {
            LoggerFactory = loggerFactory;
        }
        public RequiredServiceElementBuilder()
        {

        }

        public RequiredServiceElement Build()
        {
            return new RequiredServiceElement(LoggerFactory, Service, UpdateService);
        }
    }
}
