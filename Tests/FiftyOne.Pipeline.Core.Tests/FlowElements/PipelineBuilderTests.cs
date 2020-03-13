/* *********************************************************************
 * This Original Work is copyright of 51 Degrees Mobile Experts Limited.
 * Copyright 2020 51 Degrees Mobile Experts Limited, 5 Charlotte Close,
 * Caversham, Reading, Berkshire, United Kingdom RG4 7BY.
 *
 * This Original Work is licensed under the European Union Public Licence (EUPL) 
 * v.1.2 and is subject to its terms as set out below.
 *
 * If a copy of the EUPL was not distributed with this file, You can obtain
 * one at https://opensource.org/licenses/EUPL-1.2.
 *
 * The 'Compatible Licences' set out in the Appendix to the EUPL (as may be
 * amended by the European Commission) shall be deemed incompatible for
 * the purposes of the Work and the provisions of the compatibility
 * clause in Article 5 of the EUPL shall not apply.
 * 
 * If using the Work as, or as part of, a network application, by 
 * including the attribution notice(s) required under Article 5 of the EUPL
 * in the end user terms of the application under an appropriate heading, 
 * such notice(s) shall fulfill the requirements of that article.
 * ********************************************************************* */

using FiftyOne.Pipeline.Core.Configuration;
using FiftyOne.Pipeline.Core.Exceptions;
using FiftyOne.Pipeline.Core.FlowElements;
using FiftyOne.Pipeline.Core.Tests.HelperClasses;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;

namespace FiftyOne.Pipeline.Core.Tests.FlowElements
{
    /// <summary>
    /// Tests impacting the <see cref="PipelineBuilder"/> class
    /// </summary>
    [TestClass]
    public class PipelineBuilderTests
    {
        private PipelineBuilder _builder;
        private TestLoggerFactory _loggerFactory;

        private Mock<IFlowElement> _element = new Mock<IFlowElement>();

        private int _maxErrors = 0;
        private int _maxWarnings = 0;


        [TestInitialize]
        public void Initialise()
        {
            _loggerFactory = new TestLoggerFactory();
            _builder = new PipelineBuilder(_loggerFactory);
        }

        [TestCleanup]
        public void Cleaup()
        {
            foreach (var logger in _loggerFactory.Loggers)
            {
                logger.AssertMaxErrors(_maxErrors);
                logger.AssertMaxWarnings(_maxWarnings);
            }
        }

        /// <summary>
        /// Test that the expected exception is thrown if a disposed
        /// <see cref="IFlowElement"/> is added to the builder.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void PipelineBuilder_AddFlowElement_Disposed()
        {
            _element.Setup(e => e.IsDisposed).Returns(true);
            _builder.AddFlowElement(_element.Object);
        }

        /// <summary>
        /// Test that the expected exception is thrown if a disposed
        /// <see cref="IFlowElement"/> is added to the builder using 
        /// the AddFlowElementsParallel method.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void PipelineBuilder_AddFlowElementsParallel_Disposed()
        {
            _element = new Mock<IFlowElement>();
            _element.Setup(e => e.IsDisposed).Returns(true);
            Mock<IFlowElement> element2 = new Mock<IFlowElement>();

            _builder.AddFlowElementsParallel(element2.Object, _element.Object);
        }

        /// <summary>
        /// Test that the expected exception gets thrown if 
        /// BuildFromConfiguration is passed a null parameter.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PipelineBuilder_BuildFromConfiguration_Null()
        {
            var pipeline = _builder.BuildFromConfiguration(null);
        }

        /// <summary>
        /// Test that the Pipeline builder works as expected when building 
        /// a pipeline from configuration.
        /// The element builder used has a single mandatory parameter on
        /// the build method.
        /// </summary>
        [TestMethod]
        public void PipelineBuilder_BuildFromConfiguration_SingleMandatoryParameter()
        {
            // Create the configuration object.
            PipelineOptions opts = new PipelineOptions();
            opts.Elements = new List<ElementOptions>
            {
                new ElementOptions()
                {
                    BuilderName = "MultiplyByElementBuilder",
                    BuildParameters = new Dictionary<string, object>()
                    {
                        { "multiple", "8" }
                    }
                }
            };

            VerifyMultiplyByElementPipeline(opts);
        }

        /// <summary>
        /// Test that the Pipeline builder works as expected when building 
        /// a pipeline from configuration where the builder class name
        /// matches by the convention of adding 'Builder' to the end of 
        /// the supplied string.
        /// </summary>
        [TestMethod]
        public void PipelineBuilder_BuildFromConfiguration_ClassNameByConvention()
        {
            // Create the configuration object.
            PipelineOptions opts = new PipelineOptions();
            opts.Elements = new List<ElementOptions>
            {
                new ElementOptions()
                {
                    BuilderName = "MultiplyByElement",
                    BuildParameters = new Dictionary<string, object>()
                    {
                        { "multiple", "8" }
                    }
                }
            };

            VerifyMultiplyByElementPipeline(opts);
        }

        /// <summary>
        /// Test that the Pipeline builder works as expected when building 
        /// a pipeline from configuration where the builder class name
        /// matches by the convention of adding 'Builder' to the end of 
        /// the supplied string.
        /// </summary>
        [TestMethod]
        public void PipelineBuilder_BuildFromConfiguration_ClassNameAlternate()
        {
            // Create the configuration object.
            PipelineOptions opts = new PipelineOptions();
            opts.Elements = new List<ElementOptions>
            {
                new ElementOptions()
                {
                    BuilderName = "Multiply",
                    BuildParameters = new Dictionary<string, object>()
                    {
                        { "multiple", "8" }
                    }
                }
            };

            VerifyMultiplyByElementPipeline(opts);
        }

        /// <summary>
        /// Test that the Pipeline builder throws the expected exception
        /// when mandatory parameters are not provided by the configuration
        /// object.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(PipelineConfigurationException))]
        public void PipelineBuilder_BuildFromConfiguration_MandatoryParameterNotSet()
        {
            // Create the configuration object.
            PipelineOptions opts = new PipelineOptions();
            opts.Elements = new List<ElementOptions>
            {
                new ElementOptions()
                {
                    BuilderName = "MultiplyByElementBuilder"
                }
            };
            _maxErrors = 1;

            // Pass the configuration to the builder to create the pipeline.
            var pipeline = _builder.BuildFromConfiguration(opts);
        }

        /// <summary>
        /// Test that the Pipeline builder throws the expected exception
        /// when the mandatory parameter value cannot be parsed to the 
        /// expected type
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(PipelineConfigurationException))]
        public void PipelineBuilder_BuildFromConfiguration_MandatoryParameterWrongType()
        {
            // Create the configuration object.
            PipelineOptions opts = new PipelineOptions();
            opts.Elements = new List<ElementOptions>
            {
                new ElementOptions()
                {
                    BuilderName = "MultiplyByElementBuilder",
                    BuildParameters = new Dictionary<string, object>()
                    {
                        { "multiple", "WrongType" }
                    }
                }
            };
            _maxErrors = 1;

            // Pass the configuration to the builder to create the pipeline.
            var pipeline = _builder.BuildFromConfiguration(opts);
        }

        /// <summary>
        /// Test that the Pipeline builder throws the expected exception
        /// when the specified builder does not exist
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(PipelineConfigurationException))]
        public void PipelineBuilder_BuildFromConfiguration_NoBuilder()
        {
            // Create the configuration object.
            PipelineOptions opts = new PipelineOptions();
            opts.Elements = new List<ElementOptions>
            {
                new ElementOptions()
                {
                    BuilderName = "NoBuilder"
                }
            };
            _maxErrors = 1;

            // Pass the configuration to the builder to create the pipeline.
            var pipeline = _builder.BuildFromConfiguration(opts);
        }

        /// <summary>
        /// Test that the Pipeline builder throws the expected exception
        /// when the specified class exists but is not used for building
        /// IFlowElements
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(PipelineConfigurationException))]
        public void PipelineBuilder_BuildFromConfiguration_WrongBuilder()
        {
            // Create the configuration object.
            PipelineOptions opts = new PipelineOptions();
            opts.Elements = new List<ElementOptions>
            {
                new ElementOptions()
                {
                    BuilderName = "PipelineBuilder"
                }
            };
            _maxErrors = 1;

            // Pass the configuration to the builder to create the pipeline.
            var pipeline = _builder.BuildFromConfiguration(opts);
        }

        /// <summary>
        /// Test that the Pipeline builder works when setting an optional
        /// parameter using the full method name
        /// </summary>
        [TestMethod]
        public void PipelineBuilder_BuildFromConfiguration_OptionalParameter()
        {
            // Create the configuration object.
            PipelineOptions opts = new PipelineOptions();
            opts.Elements = new List<ElementOptions>
            {
                new ElementOptions()
                {
                    BuilderName = "ListSplitterElement",
                    BuildParameters = new Dictionary<string, object>()
                    {
                        { "SetDelimiter", "|" }
                    }
                }
            };

            VerifyListSplitterElementPipeline(opts, SplitOption.Pipe);
        }

        /// <summary>
        /// Test that the Pipeline builder works when setting an optional
        /// parameter using the convention of removing 'Set' from the start
        /// of the method name.
        /// </summary>
        [TestMethod]
        public void PipelineBuilder_BuildFromConfiguration_MethodNameConvention()
        {
            // Create the configuration object.
            PipelineOptions opts = new PipelineOptions();
            opts.Elements = new List<ElementOptions>
            {
                new ElementOptions()
                {
                    BuilderName = "ListSplitterElement",
                    BuildParameters = new Dictionary<string, object>()
                    {
                        { "Delimiter", "|" }
                    }
                }
            };

            VerifyListSplitterElementPipeline(opts, SplitOption.Pipe);
        }

        /// <summary>
        /// Test that the Pipeline builder works when setting an optional
        /// parameter using the alternate name attribute on the method.
        /// </summary>
        [TestMethod]
        public void PipelineBuilder_BuildFromConfiguration_MethodNameAlternate()
        {
            // Create the configuration object.
            PipelineOptions opts = new PipelineOptions();
            opts.Elements = new List<ElementOptions>
            {
                new ElementOptions()
                {
                    BuilderName = "ListSplitterElement",
                    BuildParameters = new Dictionary<string, object>()
                    {
                        { "delim", "|" }
                    }
                }
            };

            VerifyListSplitterElementPipeline(opts, SplitOption.Pipe);
        }

        /// <summary>
        /// Test that the Pipeline builder works when an optional parameter
        /// is left as the default value
        /// </summary>
        [TestMethod]
        public void PipelineBuilder_BuildFromConfiguration_OptionalDefault()
        {
            // Create the configuration object.
            PipelineOptions opts = new PipelineOptions();
            opts.Elements = new List<ElementOptions>
            {
                new ElementOptions()
                {
                    BuilderName = "ListSplitterElement",
                }
            };

            VerifyListSplitterElementPipeline(opts, SplitOption.Comma);
        }

        /// <summary>
        /// Test that the Pipeline builder throws an exception if the specified
        /// optional parameter method name does not exist.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(PipelineConfigurationException))]
        public void PipelineBuilder_BuildFromConfiguration_OptionalMethodMissing()
        {
            // Create the configuration object.
            PipelineOptions opts = new PipelineOptions();
            opts.Elements = new List<ElementOptions>
            {
                new ElementOptions()
                {
                    BuilderName = "ListSplitterElement",
                    BuildParameters = new Dictionary<string, object>()
                    {
                        { "NoMethod", "|" }
                    }
                }
            };
            _maxErrors = 1;

            VerifyListSplitterElementPipeline(opts, SplitOption.Comma);
        }

        /// <summary>
        /// Test that the Pipeline builder works as expected if the specified
        /// optional parameter method takes an integer
        /// </summary>
        [TestMethod]
        public void PipelineBuilder_BuildFromConfiguration_OptionalMethodInteger()
        {
            // Create the configuration object.
            PipelineOptions opts = new PipelineOptions();
            opts.Elements = new List<ElementOptions>
            {
                new ElementOptions()
                {
                    BuilderName = "ListSplitterElement",
                    BuildParameters = new Dictionary<string, object>()
                    {
                        { "MaxLength", "3" }
                    }
                }
            };

            VerifyListSplitterElementPipeline(opts, SplitOption.CommaMaxLengthThree);
        }

        /// <summary>
        /// Test that the Pipeline builder throws an exception if the specified
        /// optional parameter method takes an integer but is given a value
        /// that cannot be parsed
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(PipelineConfigurationException))]
        public void PipelineBuilder_BuildFromConfiguration_OptionalMethodWrongType()
        {
            // Create the configuration object.
            PipelineOptions opts = new PipelineOptions();
            opts.Elements = new List<ElementOptions>
            {
                new ElementOptions()
                {
                    BuilderName = "ListSplitterElement",
                    BuildParameters = new Dictionary<string, object>()
                    {
                        { "MaxLength", "WrongType" }
                    }
                }
            };
            _maxErrors = 1;

            VerifyListSplitterElementPipeline(opts, SplitOption.Comma);
        }


        /// <summary>
        /// Test that the Pipeline builder works when setting an optional
        /// parameter using the full method name
        /// </summary>
        [TestMethod]
        public void PipelineBuilder_BuildFromConfiguration_ParallelElements()
        {
            // Create the configuration object.
            PipelineOptions options = new PipelineOptions();
            options.Elements = new List<ElementOptions>
            {
                new ElementOptions()
                {
                    SubElements = new List<ElementOptions>
                    {
                        new ElementOptions()
                        {
                            BuilderName = "ListSplitterElement"
                        },
                        new ElementOptions()
                        {
                            BuilderName = "MultiplyByElement",
                            BuildParameters = new Dictionary<string, object>()
                            {
                                { "Multiple", "3" }
                            }
                        }
                    }
                }
            };

            // Pass the configuration to the builder to create the pipeline.
            var pipeline = _builder.BuildFromConfiguration(options);
            // Get the elements
            var splitterElement = pipeline.GetElement<ListSplitterElement>();
            var multiplyByElement = pipeline.GetElement<MultiplyByElement>();

            // Create, populate and process flow data.
            var flowData = pipeline.CreateFlowData();
            flowData
                .AddEvidence(splitterElement.EvidenceKeys[0], "1,2,abc")
                .AddEvidence(multiplyByElement.EvidenceKeys[0], 25)
                .Process();

            // Get the results and verify them.
            var splitterData = flowData.GetFromElement(splitterElement);
            var multiplyByData = flowData.GetFromElement(multiplyByElement);

            Assert.AreEqual("1", splitterData.Result[0]);
            Assert.AreEqual("2", splitterData.Result[1]);
            Assert.AreEqual("abc", splitterData.Result[2]);
            Assert.AreEqual(75, multiplyByData.Result);
        }

        /// <summary>
        /// Test that the Pipeline builder can get element builder instances
        /// from a service collection.
        /// </summary>
        [TestMethod]
        public void PipelineBuilder_BuildFromConfiguration_ServiceCollection()
        {
            // Create the configuration object.
            PipelineOptions opts = new PipelineOptions();
            opts.Elements = new List<ElementOptions>
            {
                new ElementOptions()
                {
                    BuilderName = "MultiplyByElementBuilder",
                    BuildParameters = new Dictionary<string, object>()
                    {
                        { "multiple", "3" }
                    }
                }
            };

            Mock<IServiceProvider> services = new Mock<IServiceProvider>();
            services.Setup(s => s.GetService(typeof(MultiplyByElementBuilder)))
                .Returns(new MultiplyByElementBuilder());

            // Pass the configuration to the builder to create the pipeline.
            var pipeline = new PipelineBuilder(_loggerFactory, services.Object)
                .BuildFromConfiguration(opts);

            // Get the element
            var multiplyByElement = pipeline.GetElement<MultiplyByElement>();

            // Create, populate and process flow data.
            var flowData = pipeline.CreateFlowData();
            flowData
                .AddEvidence(multiplyByElement.EvidenceKeys[0], 25)
                .Process();

            // Get the results and verify them.
            var multiplyByData = flowData.GetFromElement(multiplyByElement);

            Assert.AreEqual(75, multiplyByData.Result);
        }

        /// <summary>
        /// Test that the Pipeline builder throws an exception when a builder
        /// is not available in the service collection.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void PipelineBuilder_BuildFromConfiguration_NotInServiceCollection()
        {
            // Create the configuration object.
            PipelineOptions opts = new PipelineOptions();
            opts.Elements = new List<ElementOptions>
            {
                new ElementOptions()
                {
                    BuilderName = "MultiplyByElementBuilder",
                    BuildParameters = new Dictionary<string, object>()
                    {
                        { "multiple", "3" }
                    }
                }
            };

            _maxErrors = 1;

            Mock<IServiceProvider> services = new Mock<IServiceProvider>();
            services.Setup(s => s.GetService(typeof(MultiplyByElementBuilder)))
                .Returns(null);

            // Pass the configuration to the builder to create the pipeline.
            var pipeline = new PipelineBuilder(_loggerFactory, services.Object)
                .BuildFromConfiguration(opts);
        }

        private enum SplitOption { Comma, Pipe, CommaMaxLengthThree }

        private void VerifyListSplitterElementPipeline(
            PipelineOptions options,
            SplitOption splitOn)
        {
            // Pass the configuration to the builder to create the pipeline.
            var pipeline = _builder.BuildFromConfiguration(options);

            var element = pipeline.GetElement<ListSplitterElement>();
            // Check we've got the expected number of evidence keys.
            Assert.AreEqual(1, element.EvidenceKeys.Count);

            // Create, populate and process flow data.
            var flowData = pipeline.CreateFlowData();
            flowData.AddEvidence(element.EvidenceKeys[0], "123,456|789,0")
                .Process();

            // Get the result and verify it.
            var elementData = flowData.GetFromElement(element);
            switch (splitOn)
            {
                case SplitOption.Comma:
                    Assert.AreEqual("123", elementData.Result[0]);
                    Assert.AreEqual("456|789", elementData.Result[1]);
                    Assert.AreEqual("0", elementData.Result[2]);
                    break;
                case SplitOption.Pipe:
                    Assert.AreEqual("123,456", elementData.Result[0]);
                    Assert.AreEqual("789,0", elementData.Result[1]);
                    break;
                case SplitOption.CommaMaxLengthThree:
                    Assert.AreEqual("123", elementData.Result[0]);
                    Assert.AreEqual("456", elementData.Result[1]);
                    Assert.AreEqual("|78", elementData.Result[2]);
                    Assert.AreEqual("9", elementData.Result[3]);
                    Assert.AreEqual("0", elementData.Result[4]);
                    break;
                default:
                    break;
            }
        }

        private void VerifyMultiplyByElementPipeline(PipelineOptions options)
        {
            // Pass the configuration to the builder to create the pipeline.
            var pipeline = _builder.BuildFromConfiguration(options);
            var element = pipeline.GetElement<MultiplyByElement>();

            // Check we've got the expected number of evidence keys.
            Assert.AreEqual(1, element.EvidenceKeys.Count);

            // Create, populate and process flow data.
            var flowData = pipeline.CreateFlowData();
            flowData.AddEvidence(element.EvidenceKeys[0], 5)
                .Process();

            // Get the result and verify it.
            var elementData = flowData.GetFromElement(element);
            Assert.AreEqual(40, elementData.Result);
        }
    }
}
