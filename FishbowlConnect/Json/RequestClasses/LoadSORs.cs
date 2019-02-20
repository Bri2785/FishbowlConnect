using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Json.Requests
{
    public class LoadSORs :IRs
    {

        public SalesOrder SalesOrder { get; set; }


        public string requestID { get; set; }

        public string StatusCode { get; set; }

        public string StatusMessage { get; set; }

    }
}
