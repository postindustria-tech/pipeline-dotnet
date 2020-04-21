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

using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace FiftyOne.Pipeline.Web.Framework.Configuration
{
    /// <summary>
    /// Extension methods used by the Web.Framework project.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Find and add a configuration file in the web application's
        /// "App_Data" folder to the server configuration builder.
        /// </summary>
        /// <param name="config">
        /// Configuration builder to add the config file to
        /// </param>
        /// <returns>
        /// The same configuration builder
        /// </returns>
        public static IConfigurationBuilder AddPipelineConfig(this IConfigurationBuilder config)
        {
            var basePath = HttpContext.Current.Server.MapPath("~/App_Data");
            // Try possible XML files.
            foreach (var fileName in Constants.ConfigFileNames)
            {
                foreach (var extension in Constants.XmlFileExtensions)
                {
                    if (File.Exists(Path.Combine(basePath, fileName + "." + extension)))
                    {
                        return config.SetBasePath(basePath)
                            .AddXmlFile(fileName + "." + extension);
                    }
                }
            }
            // Try possible JSON files.
            foreach (var fileName in Constants.ConfigFileNames)
            {
                foreach (var extension in Constants.JsonFileExtensions)
                {
                    if (File.Exists(Path.Combine(basePath, fileName + "." + extension)))
                    {
                        return config.SetBasePath(basePath)
                            .AddJsonFile(fileName + "." + extension);
                    }
                }
            }
            return config;
        }
    }
}
