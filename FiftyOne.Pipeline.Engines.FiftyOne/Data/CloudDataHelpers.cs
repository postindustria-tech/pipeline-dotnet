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

using FiftyOne.Pipeline.Core.Data.Types;
using FiftyOne.Pipeline.Core.FlowElements;
using FiftyOne.Pipeline.Engines.Data;
using FiftyOne.Pipeline.Engines.FlowElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FiftyOne.Pipeline.Engines.FiftyOne.Data
{
    public class CloudDataHelpers
    {
        /// <summary>
        /// Try to get the value from the base dictionary and convert it to the
        /// requested type. If requesting an AspectProperty then convert to the
        /// inner type and set the no value reason if there is no value and a 
        /// no value reason has been provided from the cloud request engine.
        /// </summary>
        /// <typeparam name="T">requested type</typeparam>
        /// <param name="data">aspect data to get the value from</param>
        /// <param name="noValueReasons">
        /// dictionary of reason strings keyed on property name
        /// </param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>true if value was populated</returns>
        public static bool TryGetAspectPropertyValue<T>(
            IAspectData data,
            IDictionary<string, string> noValueReasons,
            string key,
            out T value)
        {
            if (typeof(IAspectPropertyValue).IsAssignableFrom(typeof(T)))
            {
                // Get the inner type of the AspectPropertyValue
                object obj;
                Type type = typeof(T);
                Type innerType;
                if (type == typeof(object))
                {
                    innerType = GetPropertyType(data, key);
                }
                else
                {
                    innerType = type.GenericTypeArguments[0];
                }
                
                if (data.AsDictionary().TryGetValue(key, out obj))
                {
                    try
                    {
                        IAspectPropertyValue temp = null;
                        if (innerType == typeof(string))
                        {
                            temp = CreateAspectPropertyValue<string>(key, obj, noValueReasons);
                        }
                        else if (innerType == typeof(double))
                        {
                            temp = CreateAspectPropertyValue<double>(key, obj, noValueReasons);
                        }
                        else if (innerType == typeof(int))
                        {
                            temp = CreateAspectPropertyValue<int>(key, obj, noValueReasons);
                        }
                        else if (innerType == typeof(bool))
                        {
                            temp = CreateAspectPropertyValue<bool>(key, obj, noValueReasons);
                        }
                        else if (innerType == typeof(IReadOnlyList<string>))
                        {
                            temp = CreateAspectPropertyValue<IReadOnlyList<string>>(key, obj, noValueReasons);
                        }
                        else if (innerType == typeof(JavaScript))
                        {
                            temp = CreateAspectPropertyValue<JavaScript>(key, obj, noValueReasons);
                        }
                        else
                        {
                            throw new Exception($"Unknown property type in " +
                                $"data. Property {key} has " +
                                $"type {obj.GetType().Name}");
                        }

                        value = (T)temp;
                    }
                    catch (InvalidCastException)
                    {
                        throw new Exception(
                            $"Expected property '{key}' to be of " +
                            $"type '{typeof(T).Name}' but it is " +

                            $"'{obj.GetType().Name}'");
                    }
                    return true;
                }
            }
            value = default(T);
            return false;
        }

        /// <summary>
        /// Set the value based on the type or add a no value reason if there 
        /// is no value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        /// <param name="noValueReasons">
        /// dictionary of reason strings keyed on property name
        /// </param>
        /// <returns></returns>
        private static IAspectPropertyValue CreateAspectPropertyValue<T>(string key, object obj, IDictionary<string, string> noValueReasons)
        {
            var value = new AspectPropertyValue<T>();
            if (obj == null)
            {
                var messages = noValueReasons.Where(r =>
                    r.Key == key ||
                    r.Key.Contains(".") && r.Key.Split('.')[1] == key);
                if (messages.Count() > 0)
                {
                    value.NoValueMessage = messages.First().Value;
                }
                else
                {
                    value.NoValueMessage = "No value (or reason) exists for " +
                        "the property in the cloud results.";
                }
            }
            else
            {
                value.Value = (T)obj;
            }
            return value;
        }

        private static Type GetPropertyType(IAspectData data, string propertyName)
        {
            Type type = typeof(object);
            var properties = data.Pipeline
                .ElementAvailableProperties[data.Engines[0].ElementDataKey];
            if (properties != null)
            {
                var property = properties[propertyName];
                if (property != null)
                {
                    type = property.Type;
                }
            }
            return type;
        }

    }
}
