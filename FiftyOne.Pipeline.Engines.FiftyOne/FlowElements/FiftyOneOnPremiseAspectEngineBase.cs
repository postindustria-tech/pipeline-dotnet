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

using FiftyOne.Pipeline.Core.Data;
using FiftyOne.Pipeline.Core.FlowElements;
using FiftyOne.Pipeline.Engines.Data;
using FiftyOne.Pipeline.Engines.FiftyOne.Data;
using FiftyOne.Pipeline.Engines.FlowElements;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FiftyOne.Pipeline.Engines.FiftyOne.FlowElements
{
    /// <summary>
    /// 51Degrees specific Engine base class. This adds the concept of license
    /// keys to the standard Engine base class.
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    public abstract class FiftyOneOnPremiseAspectEngineBase<T> : 
        OnPremiseAspectEngineBase<T, IFiftyOneAspectPropertyMetaData>, 
        IFiftyOneAspectEngine
        where T : IAspectData
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger">
        /// The logger to use
        /// </param>
        /// <param name="aspectDataFactory">
        /// The factory function to use when the engine creates an
        /// <see cref="AspectDataBase"/> instance.
        /// </param>
        /// <param name="tempDataFilePath">
        /// The directory to use when storing temporary copies of the 
        /// data file(s) used by this engine.
        /// </param>
        public FiftyOneOnPremiseAspectEngineBase(
            ILogger<FiftyOneOnPremiseAspectEngineBase<T>> logger, 
            Func<IPipeline, FlowElementBase<T, IFiftyOneAspectPropertyMetaData>, T> aspectDataFactory, 
            string tempDataFilePath) : 
            base(logger, aspectDataFactory, tempDataFilePath)
        {
        }

        #endregion

        #region Public Interface

        public abstract IEnumerable<IProfileMetaData> Profiles { get; }

        public abstract IEnumerable<IComponentMetaData> Components { get; }

        public abstract IEnumerable<IValueMetaData> Values { get; }

        public abstract string GetDataDownloadType(string identifier);        
        #endregion
    }
}
