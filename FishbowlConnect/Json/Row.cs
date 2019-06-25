using System;
using System.Collections.Generic;
using System.Text;
using FishbowlConnect.Json.Converters;
using Newtonsoft.Json;

namespace FishbowlConnect.Json
{

    public class Row
    {
        [JsonProperty(PropertyName ="Row")]
        [JsonConverter(typeof(ListOrSingleValueConverter<string>))]
        public List<string> RowField { get; set; }
        //public string RowField { get; set; }

    }
}
