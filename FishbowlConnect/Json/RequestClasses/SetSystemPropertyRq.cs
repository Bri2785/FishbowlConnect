using FishbowlConnect.Json.APIObjects;
using FishbowlConnect.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Json.RequestClasses
{
    public class SetSystemPropertyRq
    {
        public PropertyList PropertyList { get; set; }

    }

    public class PropertyList
    {
        [JsonConverter(typeof(ListOrSingleValueConverter<SystemProperty>))]
        public List<SystemProperty> SystemProperty { get; set; }
    }
}
