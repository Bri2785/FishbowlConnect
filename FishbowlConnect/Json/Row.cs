using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace FishbowlConnect.Json
{

    public class Row
    {
        [JsonProperty(PropertyName ="Row")]
        public string RowField { get; set; }

    }
}
