using FishbowlConnect.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Json
{
    public class Rows
    {
        [JsonConverter(typeof(ListOrSingleValueConverter<string>))]
        public List<string> Row { get; set; }
    }
}
