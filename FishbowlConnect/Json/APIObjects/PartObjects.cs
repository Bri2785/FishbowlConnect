using FishbowlConnect.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace FishbowlConnect.Json.APIObjects
{
    [Serializable()]
    public class Part
    {
        public Part()
        {
            this.HasBOM = "false";
            this.Configurable = "false";
            this.ActiveFlag = "true";
            this.SerializedFlag = "false";
            this.TrackingFlag = "false";
        }

        public string PartID { get; set; }

        public string PartClassID { get; set; }

        public string TypeID { get; set; }

        public UOM UOM { get; set; }
        public string Num { get; set; }

        public string Description { get; set; }

        public string Details { get; set; }

        public string ABCCode { get; set; }

        public string StandardCost { get; set; }

        public string HasBOM { get; set; }

        public string Configurable { get; set; }

        public string PickInPart { get; set; }

        public string ActiveFlag { get; set; }

        public string SerializedFlag { get; set; }

        public string TrackingFlag { get; set; }

        public string Weight { get; set; }

        public WeightUOM WeightUOM { get; set; }

        public string Width { get; set; }

        public string Height { get; set; }

        public string Len { get; set; }

        public SizeUOM SizeUOM { get; set; }

        public string UPC { get; set; }

        public List<CustomField> CustomFields { get; set; }

        public PartTrackingList PartTrackingList { get; set; }

        public VendorPartNums VendorPartNums { get; set; }
    }

    [Serializable()]
    public partial class PartTrackingList
    {

        [JsonConverter(typeof(ListOrSingleValueConverter<PartTracking>))]
        public List<PartTracking> PartTracking { get; set; }
    }
}
