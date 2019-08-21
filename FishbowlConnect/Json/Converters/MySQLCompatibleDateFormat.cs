using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace FishbowlConnect.Json.Converters
{
    public class MySQLCompatibleDateFormat : JsonConverter
    {
        private const string DefaultDateTimeFormat = "yyyy-MM-ddTHH:mm:ss";

        public override bool CanConvert(Type objectType)
        {
            return true; //always convert
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return reader.Value.ToString(); //dont change read

            //if (reader.TokenType == JsonToken.Null)
            //{
            //    throw new JsonSerializationException(string.Format("Cannot convert null value to {0}.", objectType.ToString()));
            //}
            //if (reader.TokenType == JsonToken.Date)
            //{
            //    return reader.Value;
            //}
            //if (reader.TokenType != JsonToken.String)
            //{
            //    throw new JsonSerializationException(string.Format("Unexpected token parsing date. Expected String, got {0}.", reader.TokenType));
            //}

            //string? dateText = reader.Value?.ToString();

            //if (string.IsNullOrEmpty(dateText))
            //{
            //    return null;
            //}

            //if (!string.IsNullOrEmpty(_dateTimeFormat))
            //{
            //    return DateTime.ParseExact(dateText, _dateTimeFormat, Culture, _dateTimeStyles);
            //}
            //else
            //{
            //    return DateTime.Parse(dateText, Culture, _dateTimeStyles);
            //}

        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            //all tracking values are strings provided by default JSON converter
            //we need to now the part tracking type to be able to specify that we want to change the string output 
            //by parsing the string to date and then re-saving to match MySQL format

            //We cant use the built in converters, because this fiedl can be any data type
            //should be an object, but then it will have to be parsed every where else

            DateTime trackingDate;
            if (DateTime.TryParse(value.ToString(), out trackingDate))
            {
                string mySQLDateString = trackingDate.ToString(DefaultDateTimeFormat);
                serializer.Serialize(writer, mySQLDateString);
            }
            else
            {
                serializer.Serialize(writer, value);
            }


        }
    }
}
