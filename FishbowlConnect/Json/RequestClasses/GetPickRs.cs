using FishbowlConnect.Json.APIObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Json.Requests
{
    public class GetPickRs:IRs
    {

        public Pick Pick { get; set; }

        public string StatusCode { get; set; }
        public string StatusMessage { get; set; }
    }
}
