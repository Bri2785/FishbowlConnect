using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Json.Requests
{
    public class ShipRq
    {
        public string ShipNum { get; set; }
        public DateTime ShipDate { get; set; }
        public bool FulfillService { get; set; }
        public string Contact { get; set; }
        public string Image { get; set; }
    }
}
