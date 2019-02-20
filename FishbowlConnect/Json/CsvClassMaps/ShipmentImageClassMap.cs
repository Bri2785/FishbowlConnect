using CsvHelper.Configuration;
using FishbowlConnect.Json.APIObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Json.CsvClassMaps
{
    public class ShipmentImageClassMap : ClassMap<ShipmentImage>

    {
        public ShipmentImageClassMap()
        {
            AutoMap();
        }
    }
}
