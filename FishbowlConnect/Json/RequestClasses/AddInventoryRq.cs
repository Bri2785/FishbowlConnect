using FishbowlConnect.Json.APIObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Json.Requests
{
    public class AddInventoryRq
    {
        public string PartNum { get; set; }
        public string Quantity { get; set; }
        public string UOMID { get; set; }
        public string Cost { get; set; }
        public string Note { get; set; }
        public List<TrackingItem> Tracking { get; set; }
        public string LocationTagNum { get; set; }
        public string TagNum { get; set; }
    }


}
