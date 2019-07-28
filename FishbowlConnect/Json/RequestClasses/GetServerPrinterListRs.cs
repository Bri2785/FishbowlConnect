using FishbowlConnect.Json.APIObjects;
using FishbowlConnect.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Json
{
    public class GetServerPrinterListRs : IRs
    {
        public Printers Printers { get; set; }
        public string StatusCode { get; set ; }
        public string StatusMessage { get ; set ; }
    }
}
