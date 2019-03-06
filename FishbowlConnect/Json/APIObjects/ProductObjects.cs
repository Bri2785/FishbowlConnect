using FishbowlConnect.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Json.APIObjects
{

    public partial class Product
    {
        public Product()
        {
            SellableInOtherUOMFlag = "true";
            ActiveFlag = "true";
            TaxableFlag = "false";
            UsePriceFlag = "true";
            KitFlag = "false";
            ShowSOComboFlag = "true";
        }

        public string ID { get; set; }

        public string PartID { get; set; }

        public Part Part { get; set; }

        public string Num { get; set; }

        /// <remarks/>
        public string Description { get; set; }

        /// <remarks/>
        public string Details { get; set; }

        /// <remarks/>
        public string UPC { get; set; }

        /// <remarks/>
        public string SKU { get; set; }

        /// <remarks/>
        public string Price { get; set; }

        /// <remarks/>
        public UOM UOM { get; set; }

        /// <remarks/>
        public string DefaultSOItemType { get; set; }

        /// <remarks/>
        public string DisplayType { get; set; }

        /// <remarks/>
        public string URL { get; set; }

        /// <remarks/>
        public string Weight { get; set; }

        /// <remarks/>
        public string WeightUOMID { get; set; }

        /// <remarks/>
        public string Width { get; set; }

        /// <remarks/>
        public string Height { get; set; }

        /// <remarks/>
        public string Len { get; set; }

        /// <remarks/>
        public string SizeUOMID { get; set; }

        /// <remarks/>
        public string AccountingID { get; set; }

        /// <remarks/>
        public string AccountingHash { get; set; }

        /// <remarks/>
        public string SellableInOtherUOMFlag { get; set; }

        /// <remarks/>
        public string ActiveFlag { get; set; }

        /// <remarks/>
        public string TaxableFlag { get; set; }

        /// <remarks/>
        public string UsePriceFlag { get; set; }

        /// <remarks/>
        public string KitFlag { get; set; }

        /// <remarks/>
        public string ShowSOComboFlag { get; set; }

        /// <remarks/>
        public string Image { get; set; }

        /// <remarks/>

        public CustomFields CustomFields { get; set; }
    }
}
