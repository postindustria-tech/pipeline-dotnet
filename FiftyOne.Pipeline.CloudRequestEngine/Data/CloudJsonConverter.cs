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

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace FiftyOne.Pipeline.CloudRequestEngine.Data
{
    /// <summary>
    /// Custom JsonConverter for use when deserialising data from 51Degrees
    /// cloud service.
    /// The converter converts integer values to Int32 instead of Int64 and
    /// converts Json arrays to <![CDATA[List<string>]]>.
    /// </summary>
    public class CloudJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Dictionary<string, object>);
        }

        public override bool CanWrite => false;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            reader.Read();

            while (reader.TokenType == JsonToken.PropertyName)
            {
                string propertyName = reader.Value as string;
                reader.Read();
                object value;

                // If the value is an array then create a new string list 
                // and add each item in turn.
                if (reader.TokenType == JsonToken.StartArray)
                {
                    var list = new List<string>();
                    reader.Read();
                    while (reader.TokenType != JsonToken.EndArray)
                    {
                        list.Add(reader.Value.ToString());
                        reader.Read();
                    }
                    value = list;
                }
                else if (reader.TokenType == JsonToken.Integer)
                    value = Convert.ToInt32(reader.Value);      // convert to Int32 instead of Int64
                else
                    value = serializer.Deserialize(reader);     // let the serializer handle all other cases

                result.Add(propertyName, value);
                reader.Read();
            }

            return result;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
