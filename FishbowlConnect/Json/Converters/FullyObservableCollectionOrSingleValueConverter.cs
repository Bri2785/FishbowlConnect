using FishbowlConnect.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Json.Converters
{
    public class FullyObservableCollectionOrSingleValueConverter<T>:JsonConverter
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
                retVal = new FullyObservableCollection<T>() { instance };
            }

            else if (reader.TokenType == JsonToken.StartArray)
            {
                retVal = serializer.Deserialize(reader, objectType);

            }
            return retVal;
            //else
            //{
            //    return new List<string> { reader.Value as string };
            //}
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            //ToDo here we can decide to write the json as 
            //if only has one attribute output as string if it has more output as list
            FullyObservableCollection<T> list = (FullyObservableCollection<T>)value;
            if (list.Count == 1)
            {
                value = list[0];
            }
            serializer.Serialize(writer, value);
        }

    }
}
