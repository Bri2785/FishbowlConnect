using FishbowlConnect.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Json.APIObjects
{
    public partial class ReportParam
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public partial class Printers
    {
        [JsonConverter(typeof(ListOrSingleValueConverter<string>))]
        public List<string> Printer { get; set; }
    }
}
