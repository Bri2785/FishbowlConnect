using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;

namespace FishbowlConnect.Json.Converters
{
    public class ListOrSingleValueConverter<T> : JsonConverter
    {

        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(T)) || (objectType == typeof(List<T>));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {

            object retVal = new Object();

            if (reader.TokenType == JsonToken.StartObject)
            {
                T instance = (T)serializer.Deserialize(reader, typeof(T));
                retVal = new List<T>() { instance };
            }

            else if (reader.TokenType == JsonToken.StartArray)
            {
                retVal = serializer.Deserialize(reader, objectType);

            }
            else
            {
                if (reader.ValueType == typeof(string))
                {
                    if (String.IsNullOrEmpty(reader.Value.ToString()))
                    {
                        return new List<T>(); //return blank list
                    }
                    else
                    {
                        return new List<T> { (T)reader.Value };
                    }
                }
            }
            return retVal;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            //ToDo here we can decide to write the json as 
            //if only has one attribute output as string if it has more output as list
            List<T> list = (List<T>)value;
            if (list.Count == 1)
            {
                value = list[0];
            }
            serializer.Serialize(writer, value);
        }

    }
}
