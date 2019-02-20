using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Json.Requests
{
    public class SOSaveRs :IRs
    {

        /// <remarks/>
        public SalesOrder SalesOrder { get; set; }

        /// <remarks/>
        public string StatusCode { get; set; }

        /// <remarks/>
        public string StatusMessage { get; set; }
    }
}
