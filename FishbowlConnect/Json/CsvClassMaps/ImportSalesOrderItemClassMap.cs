using CsvHelper.Configuration;
using FishbowlConnect.Json.Imports;
using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Json.CsvClassMaps
{
    public class ImportSalesOrderItemClassMap : ClassMap<ImportSalesOrderItem>
    {
        public ImportSalesOrderItemClassMap()
        {


            Map(m => m.Flag).Index(0);
            Map(m => m.SOItemTypeID).Index(1);
            Map(m => m.ProductNumber).Index(2);
            Map(m => m.ProductDescription).Index(3);
            Map(m => m.ProductQuantity).Index(4);
            Map(m => m.UOM).Index(5);
            Map(m => m.ProductPrice).Index(6);
            Map(m => m.Taxable).Index(7);
            Map(m => m.TaxCode).Index(8);
            Map(m => m.Note).Index(9);
            Map(m => m.QuickBooksClassName).Index(10);
            Map(m => m.ItemDateScheduled).Index(11);
            Map(m => m.ShowItem).Index(12);
            Map(m => m.KitItem).Index(13);
            Map(m => m.RevisionLevel).Index(14);
        }
    }
}
