/* *********************************************************************
 * This Original Work is copyright of 51 Degrees Mobile Experts Limited.
 * Copyright 2019 51 Degrees Mobile Experts Limited, 5 Charlotte Close,
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

using FiftyOne.Pipeline.Core.Attributes;
using FiftyOne.Pipeline.Core.Exceptions;
using FiftyOne.Pipeline.Engines.Configuration;
using FiftyOne.Pipeline.Engines.Data;
using FiftyOne.Pipeline.Engines.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FiftyOne.Pipeline.Engines.FlowElements
{
    /// <summary>
    /// Abstract base class that exposes the common options that all
    /// 51Degrees on-premise engine builders should make use of.
    /// </summary>
    /// <typeparam name="TBuilder">
    /// The specific builder type to use as the return type from the fluent 
    /// builder methods.
    /// </typeparam>
    /// <typeparam name="TEngine">
    /// The type of the engine that this builder will build
    /// </typeparam>
    public abstract class OnPremiseAspectEngineBuilderBase<TBuilder, TEngine> :
        AspectEngineBuilderBase<TBuilder, TEngine>
        where TBuilder : OnPremiseAspectEngineBuilderBase<TBuilder, TEngine>
        where TEngine : IOnPremiseAspectEngine
    {

        private IDataUpdateService _dataUpdateService;

        protected List<IDataFileConfiguration> DataFileConfigs { get; set; } = new List<IDataFileConfiguration>();

        // Used to store a temporary list of the data file 
        // meta data between their creation and the creation
        // of the engine.
        protected List<AspectEngineDataFile> DataFiles { get; set; } = new List<AspectEngineDataFile>();

        protected string TempDir { get; set; } = Path.GetTempPath();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataUpdateService">
        /// The <see cref="IDataUpdateService"/> instance to use when 
        /// checking for data updates.
        /// If null is passed then data updates functionality will be 
        /// unavailable.
        /// </param>
        public OnPremiseAspectEngineBuilderBase(
            IDataUpdateService dataUpdateService)
        {
            _dataUpdateService = dataUpdateService;
        }

        /// <summary>
        /// Add a data file for this engine to use.
        /// </summary>
        /// <param name="configuration">
        /// The data file configuration to add to this engine.
        /// <returns>
        /// This engine builder instance.
        /// </returns>
        public TBuilder AddDataFile(IDataFileConfiguration configuration)
        {
            DataFileConfigs.Add(configuration);
            return this as TBuilder;
        }
        
        /// <summary>
        /// Set the temporary path to use when the engine needs to create
        /// temporary files. (e.g. when downloading data updates)
        /// Default = Path.GetTempPath();
        /// </summary>
        /// <param name="dirPath">
        /// The full path to the temporary directory
        /// </param>
        /// <returns>
        /// This engine builder instance.
        /// </returns>
        public TBuilder SetTempDirPath(string dirPath)
        {
            TempDir = dirPath;
            return this as TBuilder;
        }

        /// <summary>
        /// Set the performance profile that the engine should use.
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public abstract TBuilder SetPerformanceProfile(PerformanceProfiles profile);

        /// <summary>
        /// Create a new instance of the <see cref="AspectEngineDataFile"/>
        /// instances used by the engine associated with this builder.
        /// If the engine uses a derived type then this method should be 
        /// overridden to return a new instance on that type.
        /// </summary>
        /// <returns>
        /// A new <see cref="AspectEngineDataFile"/> instance.
        /// </returns>
        protected virtual AspectEngineDataFile NewAspectEngineDataFile()
        {
            return new AspectEngineDataFile();
        }

        /// <summary>
        /// Called by the 'BuildEngine' method to handle
        /// anything that needs doing before the engine is built.
        /// </summary>
        protected override void PreCreateEngine()
        {
            // Register any configured files with the data update service.
            // Any files that have the 'update on startup' flag set 
            // will be updated now.
            // Create the auto-update configuration and register it.
            foreach (var dataFileConfig in DataFileConfigs)
            {
                var dataFile = NewAspectEngineDataFile();
                dataFile.Identifier = dataFileConfig.Identifier;
                dataFile.Configuration = dataFileConfig;
                dataFile.TempDataDirPath = TempDir;

                if (dataFileConfig.AutomaticUpdatesEnabled ||
                    dataFileConfig.UpdateOnStartup ||
                    dataFileConfig.FileSystemWatcherEnabled)
                {
                    if (_dataUpdateService == null)
                    {
                        List<string> features = new List<string>();
                        if (dataFileConfig.AutomaticUpdatesEnabled) { features.Add("auto update"); }
                        if (dataFileConfig.UpdateOnStartup) { features.Add("update on startup"); }
                        if (dataFileConfig.FileSystemWatcherEnabled) { features.Add("file system watcher"); }

                        throw new Exception(
                            $"Some enabled features ({string.Join(",", features)}) " +
                            "require an IDataUpdateService but one has not been supplied. " +
                            "This can be corrected by passing an IDataUpdateService " +
                            "instance to the engine builder constructor. " +
                            "If building from configuration, an IServiceProvider " +
                            "instance, able to resolve an IDataUpdateService, " +
                            "must be passed to the pipeline builder.");
                    }
                    _dataUpdateService.RegisterDataFile(dataFile);
                }
                DataFiles.Add(dataFile);
            }
        }

        /// <summary>
        /// Called by the 'BuildEngine' method to handle
        /// configuration of the engine after it is built.
        /// Can be overridden by derived classes to add additional
        /// configuration, but the base method should always be called.
        /// </summary>
        /// <param name="engine">
        /// The engine to configure.
        /// </param>
        protected override void ConfigureEngine(TEngine engine)
        {
            base.ConfigureEngine(engine);

            foreach (var dataFile in DataFiles)
            {
                dataFile.Engine = engine;
                if (engine != null)
                {
                    engine.AddDataFile(dataFile);
                }
            }
        }
    }
}
