using CsvHelper.Configuration;
using FishbowlConnect.Json.Imports;
using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Json.CsvClassMaps
{
    public class ImportProductPricingAndUpcClassMap : ClassMap<ImportProductPricingAndUpc>
    {
        public ImportProductPricingAndUpcClassMap()
        {
            Map(m => m.PartNumber).Index(0);
            Map(m => m.ProductNumber).Index(1);
            Map(m => m.Price).Index(5);
            Map(m => m.ProductUPC).Index(13);
        }
    }
}
