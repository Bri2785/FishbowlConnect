using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Json
{
    public class Module
    {
        [JsonProperty(PropertyName = "Module")]
        public string[] ModuleList { get; set; }
    }
}
