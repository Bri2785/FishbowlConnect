using FishbowlConnect.Json.APIObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Json.Requests
{
    public class GetShipmentRs : IRs
    {
        public string StatusCode { get ; set ; }
        public string StatusMessage { get ; set ; }

        public Shipping Shipping { get; set; }
    }
}
