using CsvHelper.Configuration;
using FishbowlConnect.Json.QueryClasses;
using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Json.CsvClassMaps
{
    public class LocationSimpleObjectClassMap : ClassMap<LocationSimpleObject>
    {
        public LocationSimpleObjectClassMap()
        {
            AutoMap();
            Map(m => m.LocationFullName).Ignore();
        }
    }
}
