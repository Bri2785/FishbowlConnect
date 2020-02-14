using FishbowlConnect.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Json.APIObjects
{

    public partial class UOM
    {

        /// <remarks/>
        public int UOMID { get; set; }

        /// <remarks/>
        public string Name { get; set; }

        /// <remarks/>
        public string Code { get; set; }

        /// <remarks/>
        public bool Integral { get; set; }

        /// <remarks/>
        public bool Active { get; set; }

        /// <remarks/>
        public string Type { get; set; }

        /// <remarks/>
        public UOMConversions UOMConversions { get; set; }
    }

    public partial class UOMConversions
    {
        [JsonConverter(typeof(ListOrSingleValueConverter<UOMConversion>))]
        public List<UOMConversion> UOMConversion { get; set; }
    }


    public partial class UOMConversion : NotifyOnChange
    {

        /// <remarks/>
        public int MainUOMID { get; set; }

        /// <remarks/>
        public int ToUOMID { get; set; }

        /// <remarks/>
        public string ToUOMCode { get; set; }

        /// <remarks/>
        public decimal ConversionMultiply { get; set; }

        /// <remarks/>
        public decimal ConversionFactor { get; set; }

        /// <remarks/>
        public bool ToUOMIsIntegral { get; set; }
    }
}
