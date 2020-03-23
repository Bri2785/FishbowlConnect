using CsvHelper.Configuration;
using FishbowlConnect.Json.APIObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Json.CsvClassMaps
{
    public class ImageClassMap : ClassMap<FishbowlImage>

    {
        public ImageClassMap()
        {
            AutoMap();
        }
    }
}
