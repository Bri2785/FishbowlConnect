using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect
{
    public static class ImportHeaders
    {
        public static readonly string IMPORT_PRODUCT_HEADER_NO_CUSTOM = 
            @"PartNumber,ProductNumber,ProductDescription,ProductDetails,UOM,Price,Active,Taxable,ComboBox,ProductURL,ProductUPC,ProductSKU,ProductSOItemType,Weight,WeightUOM,Width,Height,Length,sizeUOM,DefaultFlag" ;

        public static readonly string IMPORT_PRODUCT_HEADER_PRICE_UPC_ONLY = 
            @"PartNumber,ProductNumber,Price,ProductUPC" ;

    }
}

