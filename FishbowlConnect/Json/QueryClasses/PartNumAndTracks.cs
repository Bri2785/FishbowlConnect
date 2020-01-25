using System;
using System.Collections.Generic;
using System.Text;
using CsvHelper.Configuration.Attributes;
using FishbowlConnect.Interfaces;

namespace FishbowlConnect.Json.QueryClasses
{
    public partial class PartNumAndTracks : IPartTrackingFields
    {
        public string PartNumber { get; set; }

        [Default(0)]
        public int TrackingID { get; set; }
        public string TrackingAbbr { get; set; }
        public string TrackingLabel { get; set; }
        [Default(0)]
        public int TrackingSortOrder { get; set; }
        [Default(0)]
        public int TrackingTypeID { get; set; }
        [Default(false)]
        public bool TrackingPrimaryFlag { get; set; }

        public string TrackingInfo { get; set; } //used on import, not used on export query
        [Ignore]
        public bool IsPrimaryTracking { get; set ; }
    }
}
