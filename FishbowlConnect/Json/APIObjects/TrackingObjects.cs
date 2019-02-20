using FishbowlConnect.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace FishbowlConnect.Json.APIObjects
{
    [Serializable()]
    public class Tracking :NotifyOnChange
    {

        [JsonConverter(typeof(ListOrSingleValueConverter<TrackingItem>))]
        public List<TrackingItem> TrackingItem { get; set; }

        [JsonIgnore]
        public string PrimaryTrackingSummary
        {
            get { return TrackingItem?[0].TrackingSummary; }
        }
    }
}
