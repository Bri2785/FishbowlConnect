using FishbowlConnect.Json.APIObjects;
using FishbowlConnect.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Json
{

    public class InvQtyRs : IRs
    {
        public string StatusCode { get ; set ; }
        public string StatusMessage { get ; set; }

        [JsonConverter(typeof(ListOrSingleValueConverter<InvQty>))]
        public List<InvQty> InvQty { get; set; }
    }
}
