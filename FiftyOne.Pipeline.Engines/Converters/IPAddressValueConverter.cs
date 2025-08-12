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

using FiftyOne.Pipeline.Core.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Net;

namespace FiftyOne.Pipeline.Engines.Converters
{
    /// <summary>
    /// A <see cref="JsonConverter"/> that converts 
    /// <see cref="IPAddress"/> instances into JSON data.
    /// </summary>
    public class IPAddressValueConverter : JsonConverter
    {
        /// <summary>
        /// If true then this converter can be used for reading as well
        /// as writing JSON.
        /// </summary>
        public override bool CanRead => true;

        /// <summary>
        /// Returns true if the converter can convert objects of the 
        /// supplied type.
        /// </summary>
        /// <param name="objectType">
        /// The type to check against.
        /// </param>
        /// <returns>
        /// True if the converter can convert the supplied type.
        /// False if not.
        /// </returns>
        public override bool CanConvert(Type objectType)
        {
            return typeof(IPAddress).IsAssignableFrom(objectType);
        }

        private readonly string TypeValue = typeof(IPAddress).Name;
        private readonly string ValueKey = "value";

        /// <summary>
        /// Convert the data from the given reader.
        /// </summary>
        /// <param name="reader">
        /// The reader to read from.
        /// </param>
        /// <param name="objectType">
        /// The type of the object to return.
        /// </param>
        /// <param name="existingValue">
        /// ??
        /// </param>
        /// <param name="serializer">
        /// The serializer to use when reading the JSON.
        /// </param>
        /// <returns>
        /// The data object.
        /// </returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader) as JObject;
            if (token is null)
            {
                throw new JsonSerializationException($"Failed to confirm object type ({TypeValue}) -- missing '{Constants.TypeKey}'.");
            }
            var typeToken = token[Constants.TypeKey];
            if (typeToken is null)
            {
                throw new JsonSerializationException($"Failed to confirm object type ({TypeValue}) -- '{Constants.TypeKey}' is missing.");
            }
            if (typeToken.Type != JTokenType.String)
            {
                throw new JsonSerializationException($"Failed to confirm object type ({TypeValue}) -- '{Constants.TypeKey}' is not a string -- {typeToken.Type}.");
            }
            var typeString = (string)typeToken;
            if (typeString != TypeValue)
            {
                throw new JsonSerializationException($"Invalid object type {TypeValue} -- '{Constants.TypeKey}' = '{typeString}'.");
            }
            var valueToken = token[ValueKey];
            if (valueToken is null)
            {
                throw new JsonSerializationException($"Failed to get '{ValueKey}'.");
            }
            if (valueToken.Type != JTokenType.String)
            {
                throw new JsonSerializationException($"Failed to get '{ValueKey}' -- not a string -- {valueToken.Type}.");
            }
            var valueString = (string)valueToken;
            try
            {
                return IPAddress.Parse(valueString);
            }
            catch (FormatException ex)
            {
                throw new JsonSerializationException("Failed to parse IP address.", ex);
            }
        }

        /// <summary>
        /// Write the given value to JSON
        /// </summary>
        /// <param name="writer">
        /// The writer to use when writing the output.
        /// </param>
        /// <param name="value">
        /// The value to be converted to JSON
        /// </param>
        /// <param name="serializer">
        /// Not used.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if one of the supplied parameters is null.
        /// </exception>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));

            var v = value as IPAddress;
            serializer.Serialize(writer, new Dictionary<string, object>
            {
                {
                    Constants.TypeKey,
                    TypeValue
                },
                {
                    ValueKey,
                    v?.ToString()
                },
            });
        }
    }
}
