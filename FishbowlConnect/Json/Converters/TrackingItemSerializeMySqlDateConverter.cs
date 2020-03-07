﻿using FishbowlConnect.Json.APIObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FishbowlConnect.Json.Converters
{
    public class TrackingItemSerializeMySqlDateConverter : JsonConverter
    {
        private const string DefaultDateTimeFormat = "yyyy-MM-ddTHH:mm:ss";

        public override bool CanConvert(Type objectType)
        {
            return true; //always convert
        }

        public override bool CanRead => false;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException("Unnecessary because CanRead is false. The type will skip the converter.");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            //all tracking values are strings provided by default JSON converter
            //we need to know the part tracking type to be able to specify that we want to change the string output 
            //by parsing the string to date and then re-saving to match MySQL format

            //We cant use the built in converters, because this field can be any data type
            //should be an object, but then it will have to be parsed everywhere else

            TrackingItem trackingItem = value as TrackingItem;
            JObject jo = new JObject();

            if (trackingItem.PartTracking != null) //some items dont have tracking
            {
                int trackingType = trackingItem.PartTracking.TrackingTypeID;
                Type type = value.GetType();
                foreach (PropertyInfo prop in type.GetProperties())
                {
                    if (prop.CanRead)
                    {
                        if (!prop.CustomAttributes.Where(attribute => attribute.AttributeType == typeof(JsonIgnoreAttribute)).Any())
                        {
                            object propVal = prop.GetValue(value, null);
                            if (propVal != null)
                            {
                                if (prop.Name == "TrackingValue")
                                {
                                    //check and write the tracking values section
                                    switch (trackingType)
                                    {
                                        case 20:
                                        case 30:
                                            DateTime trackingDate;
                                            if (DateTime.TryParse(propVal.ToString(), out trackingDate))
                                            {
                                                string mySQLDateString = trackingDate.ToString(DefaultDateTimeFormat);
                                                jo.Add(prop.Name, JToken.FromObject(mySQLDateString, serializer));
                                            }
                                            break;

                                        default:
                                            jo.Add(prop.Name, JToken.FromObject(propVal, serializer));
                                            break;
                                    }
                                }
                                else
                                {
                                    //write the PartTracking section
                                    jo.Add(prop.Name, JToken.FromObject(propVal, serializer));
                                }
                            }
                        }
                    }
                }
                //Debug.WriteLine(jo.Property("TrackingValue")?.Value);
                jo.WriteTo(writer);

            }
            else
            {
                JToken t = JToken.FromObject(value,serializer);
                t.WriteTo(writer);
            }
        }
    }
}
