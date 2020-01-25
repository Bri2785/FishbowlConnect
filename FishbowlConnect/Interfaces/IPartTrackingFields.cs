using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Interfaces
{
    public interface IPartTrackingFields
    {
        //string PartNumber { get; set; }
        int TrackingTypeID { get; set; }
        int TrackingID { get; set; }
        string TrackingLabel { get; set; }
        string TrackingInfo { get; set; }
        int TrackingSortOrder { get; set; }
        bool IsPrimaryTracking { get; set; }
    }
}
