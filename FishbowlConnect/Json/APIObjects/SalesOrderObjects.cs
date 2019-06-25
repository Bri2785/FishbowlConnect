using FishbowlConnect.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Json.APIObjects
{


    public partial class SalesOrder : NotifyOnChange
    {
        public SalesOrder()
        {
            this.IssueFlag = "false";
        }

        public string ID { get; set; }
        public string Note { get; set; }

        public string TotalPrice { get; set; }

        public string TotalTax { get; set; }
        public string PaymentTotal { get; set; }
        public string ItemTotal { get; set; }
        public string Salesman { get; set; }
        public string Number { get; set; }
        public string Status { get; set; }
       
        [Obsolete("Use new carrierName and CarrierService properties")]
        public string Carrier { get; set; }
    
        public string FirstShipDate { get; set; }
        
        public string CreatedDate { get; set; }
        
        public string IssuedDate { get; set; }
        public string TaxRatePercentage { get; set; }
        public string TaxRateName { get; set; }

        public string ShippingCost { get; set; }
        public string ShippingTerms { get; set; }

        public string PaymentTerms { get; set; }

        public string CustomerContact { get; set; }
        public string CustomerName { get; set; }
        public string CustomerID { get; set; }
        public string FOB { get; set; }
        
        public string QuickBooksClassName { get; set; }
        public string LocationGroup { get; set; }
        public string CurrencyRate { get; set; }
        
        public string CurrencyName { get; set; }
        public string PriceIsInHomeCurrency { get; set; }
        public BillType BillTo { get; set; }
        public ShipType Ship { get; set; }
        public string IssueFlag { get; set; }
        public string VendorPO { get; set; }
        public string CustomerPO { get; set; }
        
        public string UPSServiceID { get; set; }
        
        public string TotalIncludesTax { get; set; }
        public string TypeID { get; set; }
        
        public string URL { get; set; }
        public string Cost { get; set; }
        public string DateCompleted { get; set; }
        public string DateLastModified { get; set; }
        
        public string DateRevision { get; set; }
        public string RegisterID { get; set; }
        
        public string ResidentialFlag { get; set; }
        
        public string SalesManInitials { get; set; }
        
        public string PriorityID { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        [JsonIgnore]
        public int EDITxnID { get; set; }
        [JsonIgnore]
        public string MagIncrementID { get; set; }
        [JsonIgnore]
        public uint MagEntityID { get; set; }
        [JsonIgnore]
        public string CarrierName { get; set; }
        [JsonIgnore]
        public string CarrierService { get; set; }

        [JsonConverter(typeof(ListOrSingleValueConverter<CustomField>))]
        public List<CustomField> CustomField { get; set; }

        [JsonConverter(typeof(ListOrSingleValueConverter<Memo>))]
        public List<Memo> Memo { get; set; }

        public Items Items { get; set; }
    }



    public partial class SalesOrderItem
    {

        public string ID { get; set; }
        public string ProductNumber { get; set; }
        public string SOID { get; set; }
        public string Description { get; set; }
        public string Taxable { get; set; }
        public string Quantity { get; set; }
        public string ProductPrice { get; set; }
        public string TotalPrice { get; set; }
        public string UOMCode { get; set; }
        public string ItemType { get; set; }
        public string Status { get; set; }
        public string QuickBooksClassName { get; set; }
        public string NewItemFlag { get; set; }
        public string LineNumber { get; set; }
        public string KitItemFlag { get; set; }
        public string AdjustmentAmount { get; set; }
        public string AdjustPercent { get; set; }
        public string CustomerPartNum { get; set; }
        public string DateLastFulfillment { get; set; }
        public string DateLastModified { get; set; }
        public string DateScheduledFulfillment { get; set; }
        public string ExchangeSOLineItem { get; set; }
        public string ItemAdjustID { get; set; }
        public string QtyFulfilled { get; set; }
        public string QtyPicked { get; set; }
        public string RevisionLevel { get; set; }

        public string TotalCost { get; set; }

        public string TaxableFlag { get; set; }

        [JsonIgnore]
        public List<CustomField> CustomField { get; set; }

        public string Note { get; set; }
        public string TaxCode { get; set; }
    }




    public partial class BillType
    {

        /// <remarks/>
        public string Name { get; set; }

        /// <remarks/>
        public string AddressField { get; set; }

        /// <remarks/>
        public string City { get; set; }

        /// <remarks/>
        public string Zip { get; set; }

        /// <remarks/>
        public string Country { get; set; }

        /// <remarks/>
        public string State { get; set; }
    }


    public partial class ShipType
    {

        /// <remarks/>
        public string Name { get; set; }

        /// <remarks/>
        public string AddressField { get; set; }

        /// <remarks/>
        public string City { get; set; }

        /// <remarks/>
        public string Zip { get; set; }

        /// <remarks/>
        public string Country { get; set; }

        /// <remarks/>
        public string State { get; set; }
    }
}
