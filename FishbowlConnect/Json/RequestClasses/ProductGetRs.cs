using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Json.Requests
{
    public class ProductGetRs
    {

        public Product Product { get; set; }


        public string statusCode { get; set; }

        public string statusMessage { get; set; }
    }
}
