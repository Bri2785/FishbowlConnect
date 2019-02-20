using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Json.Converters
{
    public class AddressTypeEnumToStringConverter : JsonConverter

    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(AddressType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string value = (string)serializer.Deserialize(reader, typeof(string));
            switch (value)
            {
                case "Main Office":
                    return AddressType.Main;
                case "Ship To":
                    return AddressType.Ship;
                case "Bill To":
                    return AddressType.Bill;
                case "Remit To":
                    return AddressType.Remit;
                case "Home":
                    return AddressType.Home;
                default:
                    return AddressType.Main;
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            AddressType addressType = (AddressType)value;
            string output = "Main Office";

            switch (addressType)
            {
                case AddressType.Home:
                    output = "Home";
                    break;
                case AddressType.Main:
                    output = "Main Office";
                    break;
                case AddressType.Ship:
                    output = "Ship To";
                    break;
                case AddressType.Bill:
                    output = "Bill To";
                    break;
                case AddressType.Remit:
                    output = "Remit To";
                    break;
                default:
                    break;
            }
            serializer.Serialize(writer, output);

        }
    }
}
