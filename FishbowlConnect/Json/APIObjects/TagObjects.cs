using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Json.APIObjects
{
    [Serializable()]
    public class Tag
    {
        public string TagID { get; set; }

        public string Num { get; set; }

        public string PartNum { get; set; }

        public string PartID { get; set; }

        public Location1 Location { get; set; }

        public decimal Quantity { get; set; }

        public string QuantityCommitted { get; set; }

        public string WONum { get; set; }

        public string DateCreated { get; set; }

        public Tracking Tracking { get; set; }

        public string TypeID { get; set; }

        public string AccountID { get; set; }

        public string TrackingFlag { get; set; }

    }
}
