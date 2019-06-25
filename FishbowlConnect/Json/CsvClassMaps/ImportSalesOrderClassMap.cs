using CsvHelper.Configuration;
using FishbowlConnect.Json.Imports;
using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Json.CsvClassMaps 
{
    public class ImportSalesOrderClassMap : ClassMap<ImportSalesOrder>
    {
        public ImportSalesOrderClassMap()
        {
            Map(m => m.Flag).Index(0);
            Map(m => m.SONum).Index(1);
            Map(m => m.Status).Index(2);
            Map(m => m.CustomerName).Index(3);
            Map(m => m.CustomerContact).Index(4);
            Map(m => m.BillToName).Index(5);
            Map(m => m.BillToAddress).Index(6);
            Map(m => m.BillToCity).Index(7);
            Map(m => m.BillToState).Index(8);
            Map(m => m.BillToZip).Index(9);
            Map(m => m.BillToCountry).Index(10);
            Map(m => m.ShipToName).Index(11);
            Map(m => m.ShipToAddress).Index(12);
            Map(m => m.ShipToCity).Index(13);
            Map(m => m.ShipToState).Index(14);
            Map(m => m.ShipToZip).Index(15);
            Map(m => m.ShipToCountry).Index(16);
            Map(m => m.ShipToResidential).Index(17);
            Map(m => m.CarrierName).Index(18);
            Map(m => m.TaxRateName).Index(19);
            Map(m => m.PriorityId).Index(20);
            Map(m => m.PONum).Index(21);
            Map(m => m.VendorPONum).Index(22);
            Map(m => m.Date).Index(23);
            Map(m => m.Salesman).Index(24);
            Map(m => m.ShippingTerms).Index(25);
            Map(m => m.PaymentTerms).Index(26);
            Map(m => m.FOB).Index(27);
            Map(m => m.Note).Index(28);
            Map(m => m.QuickbooksClassName).Index(29);
            Map(m => m.LocationGroupName).Index(30);
            Map(m => m.OrderDateScheduled).Index(31);
            Map(m => m.URL).Index(32);
            Map(m => m.CarrierService).Index(33);
            Map(m => m.DateExpired).Index(34);
            Map(m => m.Phone).Index(35);
            Map(m => m.Email).Index(36);
        }
    }
}
