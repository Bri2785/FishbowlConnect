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

        //public override bool CanRead => false;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            //throw new NotImplementedException("Unnecessary because CanRead is false. The type will skip the converter.");
            //base.ReadJson(reader, objectType, existingValue, serializer);
            return reader.Value.ToString(); //dont change read

        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            //all tracking values are strings provided by default JSON converter
            //we need to know the part tracking type to be able to specify that we want to change the string output 
            //by parsing the string to date and then re-saving to match MySQL format

            //We cant use the built in converters, because this field can be any data type
            //should be an object, but then it will have to be parsed everywhere else

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
