using CsvHelper.Configuration;
using FishbowlConnect.QueryClasses;
using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Json.CsvClassMaps
{
    public class TagTrackingItemClassMap : ClassMap<TagTrackingObject>
    {
        public TagTrackingItemClassMap()
        {
            Map(m => m.TagID).Index(0);
            Map(m => m.Info).Index(1);
            Map(m => m.TrackingLabel).Index(2);
            Map(m => m.TrackingTypeID).Index(3);
        }

    }
}
