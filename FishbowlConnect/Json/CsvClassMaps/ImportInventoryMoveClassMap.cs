using CsvHelper.Configuration;
using FishbowlConnect.Json.Imports;
using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Json.CsvClassMaps
{
    public sealed class ImportInventoryMoveClassMap : ClassMap<ImportInventoryMove>
    {
        public ImportInventoryMoveClassMap()
        {
            
            AutoMap();
            Map(m => m.TrackingFieldName).Ignore();
        }
    }
}
