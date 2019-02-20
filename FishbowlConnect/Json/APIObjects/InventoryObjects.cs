using FishbowlConnect.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Json.APIObjects
{

    public partial class InvQty
    {

        /// <remarks/>
        public Part Part { get; set; }

        /// <remarks/>
        public Location1 Location { get; set; }

        /// <remarks/>
        public string QtyOnHand { get; set; }

        /// <remarks/>
        public string QtyAvailable { get; set; }

        /// <remarks/>
        public string QtyCommitted { get; set; }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("TrackingItem", IsNullable = false)]
        [JsonConverter(typeof(ListOrSingleValueConverter<TrackingItem>))]
        public List<TrackingItem> Tracking { get; set; }
    }
}
