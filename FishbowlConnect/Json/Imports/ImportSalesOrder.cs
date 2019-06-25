using FishbowlConnect.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Json.Imports
{
    public class ImportSalesOrder
    {
        //fields for the Sales order to be imported
        public string Flag { get; set; }
        public string SONum { get; set; }
        public string Status { get; set; }
        public string CustomerName { get; set; }
        public string CustomerContact { get; set; }
        public string BillToName { get; set; }
        public string BillToAddress { get; set; }
        public string BillToCity { get; set; }
        public string BillToState { get; set; }
        public string BillToZip { get; set; }
        public string BillToCountry { get; set; }
        public string ShipToName { get; set; }
        public string ShipToAddress { get; set; }
        public string ShipToCity { get; set; }
        public string ShipToState { get; set; }
        public string ShipToZip { get; set; }
        public string ShipToCountry { get; set; }
        public string ShipToResidential { get; set; }
        public string CarrierName { get; set; }
        public string TaxRateName { get; set; }
        public string PriorityId { get; set; }
        public string PONum { get; set; }
        public string VendorPONum { get; set; }
        /// <summary>
        /// Needs to be in "MM/dd/yyyy" format
        /// </summary>
        public string Date { get; set; }
        public string Salesman { get; set; }
        public string ShippingTerms { get; set; }
        public string PaymentTerms { get; set; }
        public string FOB { get; set; }
        public string Note { get; set; }
        public string QuickbooksClassName { get; set; }
        public string LocationGroupName { get; set; }
        public string OrderDateScheduled { get; set; }
        public string URL { get; set; }
        public string CarrierService { get; set; }
        public string DateExpired { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }


        //Here will be dynamic custom field list, use header row and available fields to get the indexes for writing


    }
}
