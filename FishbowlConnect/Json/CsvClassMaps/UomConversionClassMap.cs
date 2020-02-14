using CsvHelper.Configuration;
using FishbowlConnect.Json.APIObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Json.CsvClassMaps
{
    public class UomConversionClassMap : ClassMap<UOMConversion>
    {
        public UomConversionClassMap()
        {
            Map(m => m.MainUOMID).Index(3);
            Map(m => m.ToUOMID).Index(5);
            Map(m => m.ConversionFactor).Index(2);
            Map(m => m.ConversionMultiply).Index(4);
            Map(m => m.ToUOMCode).Index(7);
            Map(m => m.ToUOMIsIntegral).Index(6);
        }
    }
}
