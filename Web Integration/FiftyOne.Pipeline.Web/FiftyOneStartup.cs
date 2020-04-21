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

using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.FileProviders;
using FiftyOne.Pipeline.Web.Services;
using FiftyOne.Pipeline.Core.FlowElements;
using FiftyOne.Pipeline.Core.Configuration;
using FiftyOne.Pipeline.Core.Exceptions;
using System.Net.Http;
using System.Linq;
using FiftyOne.Pipeline.JavaScriptBuilder.FlowElement;
using FiftyOne.Pipeline.JsonBuilder.FlowElement;
using System.Collections.Generic;
using FiftyOne.Pipeline.Engines.FiftyOne.FlowElements;

namespace FiftyOne.Pipeline.Web
{
    /// <summary>
    /// Static class used to setup services, configure DI, etc.
    /// </summary>
    public static class FiftyOneStartup
    {
        /// <summary>
        /// Set up the specified <see cref="IServiceCollection"/> with the 
        /// services that will be needed by device detection.
        /// This is called from the IServiceCollection.AddFiftyOne() 
        /// extension method.
        /// </summary>
        /// <param name="services">
        /// The service collection to add the Pipeline to
        /// </param>
        /// <param name="configuration">
        /// The configuration object
        /// </param>
        /// <param name="pipelineFactory">
        /// The function used to create the pipeline. If null is passed 
        /// then this will default to the CreatePipelineFromConfig method
        /// </param>
        internal static void ConfigureServices<TBuilder>(IServiceCollection services, 
            IConfiguration configuration,
            Func<IConfiguration, IPipelineBuilderFromConfiguration, IPipeline> pipelineFactory)
            where TBuilder : class, IPipelineBuilderFromConfiguration
        {
            services.AddLogging();
            services.AddOptions();

#if NETCOREAPP2_0 || NETCOREAPP2_1 || NETCOREAPP2_2
            // Set up a file provider that will allow the client web 
            // application to find the 51D views embedded in our assembly
            // when it needs them
            services.Configure<RazorViewEngineOptions>(o =>
            {
                o.FileProviders.Add(new EmbeddedFileProvider(
                    typeof(FiftyOneJSViewComponent).GetTypeInfo().Assembly,
                    "FiftyOne.Pipeline.Web"));
            });
#else
            // Set up a file provider that will allow the client web 
            // application to find the 51D views embedded in our assembly
            // when it needs them
            services.AddMvc().AddRazorRuntimeCompilation(o =>
            {
                o.FileProviders.Add(new EmbeddedFileProvider(
                    typeof(FiftyOneJSViewComponent).Assembly, 
                    "FiftyOne.Pipeline.Web"));
            });
#endif

            // Set up our DI mappings
            services.AddSingleton<HttpClient>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IFlowDataProvider, FlowDataProvider>();
            services.AddSingleton<IClientsidePropertyService,
                ClientsidePropertyService>();
            services.AddSingleton<IFiftyOneJSService, FiftyOneJSService>();
            services.AddSingleton<IPipelineResultService,
                PipelineResultService>();
            services.AddSingleton<IWebRequestEvidenceService,
                WebRequestEvidenceService>();
            services.AddTransient<IPipelineBuilderFromConfiguration, TBuilder>();
            services.AddSingleton<SequenceElementBuilder>();
            services.AddSingleton<JsonBuilderElementBuilder>();
            services.AddSingleton<JavaScriptBuilderElementBuilder>();

            // Add the pipeline to the DI container
            services.AddSingleton(serviceProvider =>
            {
                IPipeline pipeline = null;
                // Create the pipeline builder
                var pipelineBuilder = 
                    serviceProvider.GetRequiredService<IPipelineBuilderFromConfiguration>();

                if (pipelineFactory == null)
                {
                    // If no factory method was provided then use the default
                    // to create the pipeline from configuration.
                    pipeline = CreatePipelineFromConfig(
                        configuration,
                        pipelineBuilder);
                }
                else
                {
                    // If a factory method was provided then use it to create
                    // the pipeline.
                    pipeline = pipelineFactory(configuration, pipelineBuilder);
                }

                return pipeline;
            });
        }
        
        /// <summary>
        /// The default <see cref="IPipeline"/> factory function.
        /// This looks for a 'PipelineOptions' configuration item and uses
        /// that to build the pipeline.
        /// </summary>
        /// <param name="config">
        /// The application configuration object
        /// </param>
        /// <param name="pipelineBuilder">
        /// an <see cref="IPipelineBuilder"/> instance
        /// </param>
        /// <returns>
        /// A new <see cref="IPipeline"/> instance
        /// </returns>
        private static IPipeline CreatePipelineFromConfig(
            IConfiguration config,
            IPipelineBuilderFromConfiguration pipelineBuilder)
        {
            // Get Pipeline options to check that it is present
            PipelineOptions options = new PipelineOptions();
            config.Bind("PipelineOptions", options);

            if (options == null ||
                options.Elements == null ||
                options.Elements.Count == 0)
            {
                throw new PipelineConfigurationException(
                    "Could not find pipeline configuration information");
            }

            PipelineWebIntegrationOptions webOptions = new PipelineWebIntegrationOptions();
            config.Bind("PipelineWebIntegrationOptions", webOptions);

            // Add the sequence element.
            var sequenceConfig = options.Elements.Where(e =>
                e.BuilderName.Contains(nameof(SequenceElement),
                    StringComparison.OrdinalIgnoreCase));
            if (sequenceConfig.Count() == 0)
            {
                // The sequence element is not included so add it.
                // Make sure it's added as the first element.
                options.Elements.Insert(0, new ElementOptions()
                {
                    BuilderName = nameof(SequenceElement)
                });
            }

            if (webOptions.ClientSideEvidenceEnabled)
            {
                // Client-side evidence is enabled so make sure the 
                // JsonBuilderElement and JavaScriptBundlerElement has been 
                // included.
                var jsonConfig = options.Elements.Where(e =>
                    e.BuilderName.Contains(nameof(JsonBuilderElement),
                        StringComparison.OrdinalIgnoreCase));
                var javascriptConfig = options.Elements.Where(e =>
                    e.BuilderName.Contains(nameof(JavaScriptBuilderElement),
                        StringComparison.OrdinalIgnoreCase));

                var jsIndex = javascriptConfig.Count() > 0 ? 
                    options.Elements.IndexOf(javascriptConfig.First()) : -1;

                if (jsonConfig.Count() == 0)
                {
                    // The json builder is not included so add it.
                    var newElementOptions = new ElementOptions()
                    {
                        BuilderName = nameof(JsonBuilderElement)
                    };
                    if (jsIndex > -1)
                    {
                        // There is already a javascript builder element
                        // so insert the json builder before it.
                        options.Elements.Insert(jsIndex, newElementOptions);
                    }
                    else
                    {
                        options.Elements.Add(newElementOptions);
                    }
                }

                if (jsIndex == -1)
                {
                    // The builder is not included so add it.
                    options.Elements.Add(new ElementOptions()
                    {
                        BuilderName = nameof(JavaScriptBuilderElement),
                        BuildParameters = new Dictionary<string, object>() {
                            { "EnableCookies", true }
                        }
                    });
                }
            }

            return pipelineBuilder.BuildFromConfiguration(options);
        }
    }
}
