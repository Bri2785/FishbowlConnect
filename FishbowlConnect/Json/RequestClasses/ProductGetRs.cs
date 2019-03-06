using FishbowlConnect.Json.APIObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Json.Requests
{
    public class ProductGetRs : IRs
    {

        public Product Product { get; set; }


        public string StatusCode { get; set; }

        public string StatusMessage { get; set; }
    }
}
