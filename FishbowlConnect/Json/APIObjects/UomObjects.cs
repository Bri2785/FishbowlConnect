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
        public string UOMID { get; set; }

        /// <remarks/>
        public string Name { get; set; }

        /// <remarks/>
        public string Code { get; set; }

        /// <remarks/>
        public string Integral { get; set; }

        /// <remarks/>
        public string Active { get; set; }

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
        public string MainUOMID { get; set; }

        /// <remarks/>
        public string ToUOMID { get; set; }

        /// <remarks/>
        public string ToUOMCode { get; set; }

        /// <remarks/>
        public string ConversionMultiply { get; set; }

        /// <remarks/>
        public string ConversionFactor { get; set; }

        /// <remarks/>
        public string ToUOMIsIntegral { get; set; }
    }
}
