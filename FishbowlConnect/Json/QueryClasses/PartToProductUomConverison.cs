using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Json.QueryClasses
{

    /// <summary>
    /// Holds values to convert from parts UOM to a products UOM
    /// </summary>
    public class PartToProductUomConversion
    {
        public string PartNumber { get; set; }
        public string ProductNumber { get; set; }
        public string ProductUpc { get; set; }
        public double Factor { get; set; }
        public double Multiply { get; set; }
        public string ProductUomCode { get; set; }

    }
}
