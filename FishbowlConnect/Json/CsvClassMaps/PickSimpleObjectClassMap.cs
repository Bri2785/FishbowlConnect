using CsvHelper.Configuration;
using FishbowlConnect.Json.QueryClasses;
using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Json.CsvClassMaps
{
    public class PickSimpleObjectClassMap : ClassMap<PickSimpleObject>
    {
        public PickSimpleObjectClassMap()
        {
            AutoMap();
            Map(m => m.PickFulfillibility).Ignore();

        }
    }
}
