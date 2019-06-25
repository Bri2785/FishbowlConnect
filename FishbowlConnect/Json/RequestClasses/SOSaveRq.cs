using FishbowlConnect.Json.APIObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Json.Requests
{
    public class SOSaveRq
    {

        /// <remarks/>
        public SalesOrder SalesOrder { get; set; }

        /// <remarks/>
        public string IssueFlag { get; set; }

        /// <remarks/>
        public string IgnoreItems { get; set; }
    }
}
