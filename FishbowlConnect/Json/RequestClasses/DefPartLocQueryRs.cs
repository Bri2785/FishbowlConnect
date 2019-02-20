using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Json.Requests
{
    public class DefPartLocQueryRs : IRs
    {

        public Location1 Location { get; set; }

        public string StatusCode { get; set; }

        public string StatusMessage { get; set; }
    }
}
