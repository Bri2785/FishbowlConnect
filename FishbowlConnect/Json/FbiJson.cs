using FishbowlConnect.Json.RequestClasses;
using FishbowlConnect.Json.Requests;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Json
{
    public class JsonWrapper
    {
        public FbiJson FbiJson { get; set; }

    }

    public class FbiJson 
    {
        public Ticket Ticket { get; set; }
        public FbiMsgsRq FbiMsgsRq { get; set; }
        public FbiMsgsRs FbiMsgsRs { get; set; }

    }
    public class Ticket
    {
        public int UserID { get; set; }
        public string Key { get; set; }
    }

    public class FbiMsgsRq
    {
        public object Rq { get; set; } //set the type and name on serialization
    }

    public class FbiMsgsRs
    {

        public IRs Rs  { get; private set; } 

        public LoginRs LoginRs {set { Rs = value; } }
        public ImportRs ImportRs { set { Rs = value; } }
        public ImportHeaderRs ImportHeaderRs { set { Rs = value; } }
        public ExecuteQueryRs ExecuteQueryRs{ set { Rs = value; } }
        public AddInventoryRs AddInventoryRs { set { Rs = value; } }
        public CycleCountRs CycleCountRs { set { Rs = value; } }
        public DefPartLocQueryRs DefPartLocQueryRs { set { Rs = value; } }
        public GetPickRs GetPickRs { set { Rs = value; } }
        public SavePickRs SavePickRs { set { Rs = value; } }
        public TagQueryRs TagQueryRs { set { Rs = value; } }
        public LoadSORs LoadSORs { set { Rs = value; } }
        public MakePaymentRs MakePaymentRs { set { Rs = value; } }
        public SOSaveRs SaveSORs { set { Rs = value; } }
        public CustomerSaveRs CustomerSaveRs { set { Rs = value; } }
        public GetShipmentRs GetShipmentRs { set { Rs = value; } }
        public SaveShipmentRs SaveShipmentRs { set { Rs = value; } }
        public ShipRs ShipRs { set { Rs = value; } }
        public InvQtyRs InvQtyRs { set { Rs = value; } }
        public ProductGetRs ProductGetRs { set { Rs = value; } }
        public PrintReportToPrinterRs PrintReportToPrinterRs { set { Rs = value; } }
        public GetServerPrinterListRs GetServerPrinterListRs { set { Rs = value; } }
        public SetSystemPropertyRs SetSystemPropertyRs { set { Rs = value; } }
        public VoidPickRs VoidPickRs { set { Rs = value; } }
        public VoidPickItemsRs VoidPickItemsRs { set { Rs = value; } }
        public SaveApiImageRs SaveApiImageRs { set { Rs = value; } }
        public string StatusCode { get; set; }
        public string StatusMessage { get; set; }
    }
}
