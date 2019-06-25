using FishbowlConnect.Json.Converters;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace FishbowlConnect.Json
{
    public class Header
    {
        [JsonConverter(typeof(ListOrSingleValueConverter<string>))]
        public List<string> Row { get; set; }
    }
}