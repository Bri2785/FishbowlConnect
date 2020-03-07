using FishbowlConnect.Json.APIObjects;
using FishbowlConnect.Json.QueryClasses;
using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Helpers
{
    public static class ObjectConverters
    {
        public static TrackingItem PartNumAndTracksToTrackingItem(PartNumAndTracks partNumAndTracks)
        {

            return new TrackingItem
            {
                TrackingValue = partNumAndTracks.TrackingInfo,
                PartTracking = new PartTracking
                {
                    Name = partNumAndTracks.TrackingLabel,
                    TrackingTypeID = partNumAndTracks.TrackingTypeID,
                    PartTrackingID = partNumAndTracks.TrackingID.ToString(),
                    Abbr = partNumAndTracks.TrackingAbbr, 
                    Primary = partNumAndTracks.TrackingPrimaryFlag.ToString()
                     
                }
            };



        }

    }
}
