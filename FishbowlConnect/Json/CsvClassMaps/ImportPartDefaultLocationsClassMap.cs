using CsvHelper.Configuration;
using FishbowlConnect.Json.Imports;
using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Json.CsvClassMaps
{
    public class ImportPartDefaultLocationsClassMap: ClassMap<ImportPartDefaultLocations>
    {
        public ImportPartDefaultLocationsClassMap()
        {
            AutoMap();
        }
    }
}
