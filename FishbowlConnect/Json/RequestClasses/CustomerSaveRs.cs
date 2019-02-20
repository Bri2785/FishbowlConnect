using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Json.Requests
{
    public class CustomerSaveRs :IRs
    {

        /// <remarks/>
        public Customer Customer { get; set; }

 
        public string StatusCode { get; set; }

        public string StatusMessage { get; set; }
    }
}
