/* *********************************************************************
 * This Original Work is copyright of 51 Degrees Mobile Experts Limited.
 * Copyright 2023 51 Degrees Mobile Experts Limited, Davidson House,
 * Forbury Square, Reading, Berkshire, United Kingdom RG1 3EU.
 *
 * This Original Work is licensed under the European Union Public Licence
 * (EUPL) v.1.2 and is subject to its terms as set out below.
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
using System.IO;
using System.Linq;

namespace FiftyOne.Pipeline.Web.Common.Tests
{
    /// <summary>
    /// Used to get values from an appsettings config file.
    /// This avoids the need to update tests when values in the config 
    /// file are updated.
    /// </summary>
    public static class ConfigHelper
    {
        /// <summary>
        /// Finds the first directory which contains the relative path and then
        /// returns the full path.
        /// </summary>
        /// <param name="relative"></param>
        /// <returns></returns>
        public static string FindFile(string relative)
        {
            var directory = FindDirectory(d => File.Exists(
                Path.Combine(d.FullName, relative)));
            if (directory != null)
            {
                return Path.Combine(directory.FullName, relative);
            }
            throw new ArgumentException(String.Format(
                "Relative path '{0}' not found in directory hierarchy " +
                "from '{1}'", 
                relative,
                Directory.GetCurrentDirectory()));
        }

        /// <summary>
        /// Returns the first directory in the ancestory where the condition 
        /// is true, or null if there is no parent directory that satisfies
        /// the condition.
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public static DirectoryInfo FindDirectory(
            Func<DirectoryInfo, bool> condition)
        {
            var current = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (current != null)
            {
                if (condition(current))
                {
                    return current;
                }
                current = current.Parent;
            }
            return null;
        }

        public static IConfigurationSection GetElementSection(
            this IConfigurationRoot root, string elementName)
        {
            var elements = root.GetSection("PipelineOptions:Elements");
            var elementSection = elements.GetChildren()
                .Where(s => s.GetChildren()
                    .Any(t => t.Key == "BuilderName" && 
                        t.Value == elementName));
            return elementSection.SingleOrDefault();
        }
    }
}
