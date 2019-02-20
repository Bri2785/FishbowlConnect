using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Json.Imports
{
    
    public class ImportInventoryMove
    {
        public string PartNumber { get; set; }
        public string BeginLocation { get; set; } //Include LocationGroup, full Location
        public int Qty { get; set; }
        public string EndLocation { get; set; }
        public string Note { get; set; }

        //FieldName is Tracking- and the actual field name. We'll need to set this name dynamically 
        public string TrackingFieldName { get; set; }
        public string TrackingValue { get; set; }

    }
}
