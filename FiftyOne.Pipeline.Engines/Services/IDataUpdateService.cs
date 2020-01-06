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

using FiftyOne.Pipeline.Engines.Configuration;
using FiftyOne.Pipeline.Engines.Data;
using FiftyOne.Pipeline.Engines.FlowElements;
using System;

namespace FiftyOne.Pipeline.Engines.Services
{
    /// <summary>
    /// Service that manages updates to data files that are used by
    /// <see cref="IOnPremiseAspectEngine"/> instances.
    /// </summary>
    public interface IDataUpdateService
    {
        /// <summary>
        /// Register an data file for automatic updates.
        /// </summary>
        /// <param name="dataFile">
        /// The details of the data file to register.
        /// </param>
        void RegisterDataFile(AspectEngineDataFile dataFile);
        /// <summary>
        /// Unregister a data file.
        /// </summary>
        /// <param name="dataFile">
        /// The data file to unregister
        /// </param>
        void UnRegisterDataFile(AspectEngineDataFile dataFile);
        /// <summary>
        /// Check if there are updates for the specified engine.
        /// </summary>
        /// <param name="engine">
        /// The engine to check for
        /// </param>
        /// <param name="dataFileIdentifier">
        /// The identifier of the data file to check for.
        /// If the engine has only one data file then this parameter is ignored.
        /// </param>
        /// <returns>
        /// The result of the update check.
        /// </returns>
        AutoUpdateStatus CheckForUpdate(
            IOnPremiseAspectEngine engine,
            string dataFileIdentifier = null);
        /// <summary>
        /// Update the specified data file from a byte[] held in memory.
        /// </summary>
        /// <param name="dataFile">
        /// The data file to update.
        /// </param>
        /// <param name="data">
        /// The data file to update with.
        /// </param>
        AutoUpdateStatus UpdateFromMemory(AspectEngineDataFile dataFile, byte[] data);
    }


    public class DataUpdateCompleteArgs : EventArgs
    {
        public bool UpdateApplied { get; set; }
        public AspectEngineDataFile DataFile { get; set; }
    }


    public enum AutoUpdateStatus
    {
        /// <summary>
        /// Update completed successfully. 
        /// </summary>
        AUTO_UPDATE_SUCCESS,
        /// <summary>
        /// HTTPS connection could not be established. 
        /// </summary>
        AUTO_UPDATE_HTTPS_ERR,
        /// <summary>
        /// No need to perform update. 
        /// </summary>
        AUTO_UPDATE_NOT_NEEDED,
        /// <summary>
        /// Update currently under way. 
        /// </summary>
        AUTO_UPDATE_IN_PROGRESS,
        /// <summary>
        /// Path to master file is directory not file
        /// </summary>
        AUTO_UPDATE_MASTER_FILE_CANT_RENAME,
        /// <summary>
        /// 51Degrees server responded with 429: too many attempts. 
        /// /// </summary>
        AUTO_UPDATE_ERR_429_TOO_MANY_ATTEMPTS,
        /// <summary>
        /// 51Degrees server responded with 403 meaning key is blacklisted. 
        /// </summary>
        AUTO_UPDATE_ERR_403_FORBIDDEN,
        /// <summary>
        /// Used when IO oerations with input or output stream failed. 
        /// </summary>
        AUTO_UPDATE_ERR_READING_STREAM,
        /// <summary>
        /// MD5 validation failed 
        /// </summary>
        AUTO_UPDATE_ERR_MD5_VALIDATION_FAILED,
        /// <summary>
        /// The new data file can't be renamed to replace the previous one.
        /// </summary>
        AUTO_UPDATE_NEW_FILE_CANT_RENAME,
        /// <summary>
        /// Refreshing the engine with the new data caused an error to occur.
        /// </summary>
        AUTO_UPDATE_REFRESH_FAILED,
        /// <summary>
        /// There was no data file configuration matching the specified
        /// identifier.
        /// </summary>
        AUTO_UPDATE_NO_CONFIGURATION,
        /// <summary>
        /// There was an error while checking for an update that was not
        /// anticipated by the developers of this service.
        /// </summary>
        AUTO_UPDATE_UNKNOWN_ERROR
    }
}
